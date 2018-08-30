##################################################################################################
#This script downloads the latest nightly InfluxDB build, starts the db engine, and creates basic
#data structures so that 'show databases' query will not fail during tests if the query gets 
#executed before CreateDatabase test
#
##################################################################################################
Stop-Process -name influxd -Force -ErrorAction SilentlyContinue


$nightlies = $true
if ($nightlies -ne $true) {
    $archive = if ($IsLinux) { "influxdb-1.2.4_linux_amd64.tar.gz"} else {"influxdb-1.2.4_windows_amd64.zip"}        
    $source = "https://dl.influxdata.com/influxdb/releases/$archive"
}
else {
    $archive = if ($IsLinux) { "influxdb-nightly_linux_amd64.tar.gz"} else {"influxdb-nightly_windows_amd64.zip"}
    $source = "https://dl.influxdata.com/influxdb/nightlies/$archive"
}
    

$archivepath = "$HOME/$archive"
$influx = "$HOME/influxdb"
$influxdata = "$HOME/.influxdb"

write-output "Download latest Influx build"	
if (!(test-path $archivepath) -or ((Get-ItemProperty -Path $archivepath -Name LastWriteTime).lastwritetime -lt $(get-date).AddDays(-1)))
{	Invoke-WebRequest $source -OutFile $archivepath }
	
if (test-path $influx)
{	remove-item -recurse $influx -Force}


write-output "Setup Influx"
#dependent on https://github.com/PowerShell/PowerShell/issues/2740
if ($IsLinux) {
    mkdir $influx
    tar xf $archivepath -C $influx
    $influxd = find $influx | grep bin/influxd
    $influxconf = find $influx | grep influxdb.conf
    $contentType = "application/x-www-form-urlencoded"
}
else {
    Expand-Archive -LiteralPath $archivepath -DestinationPath $influx -Force
    $influxd = Get-ChildItem $influx -File -filter "influxd.exe" -Recurse | % { $_.FullName }
    $influxconf = Get-ChildItem $influx -File -filter "influxdb.conf" -Recurse | % { $_.FullName }
    $contentType = "application/x-www-form-urlencoded; charset=utf-8"
}


$cred = Get-Content .\tests\cred.json -Encoding UTF8 | ConvertFrom-Json
#$cred = @{User="admin"; Password="test123$€₹#₳₷ȅ"}
$authHeader = @{Authorization = "Basic $([System.Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes("$($cred.User):$($cred.Password)")))"}

#$x = 7z e $archivepath -o"$env:Temp/influxdb" -y
if (test-path $influxdata)
{ remove-item -recurse $influxdata -Force}

#regenerate config
Start-Process -FilePath $influxd -ArgumentList "config" -RedirectStandardOutput $influxconf
Start-Sleep -s 5

write-output "Run influxd without Auth"
#start without any auth
$proc = Start-Process -FilePath $influxd -ArgumentList "-config $influxconf" -PassThru
Start-Sleep -s 5

$r = Invoke-WebRequest -Method Post -Uri "http://localhost:8086/query?" -Body "q=CREATE DATABASE prereq" -UseBasicParsing
if ($r.StatusCode -ne 200) {
    throw "Unable to create DB"
}
write-output "created DB"

$r = Invoke-WebRequest -Method Post -Uri "http://localhost:8086/write?db=prereq" -Body "test,TestDate=$($(get-date).ToString('yyyyMMdd')),TestTime=$($(get-date).ToString('hhmm')) value=1234" -UseBasicParsing
if ($r.StatusCode -ne 204) {
    throw "Unable to create Measurement"
}
write-output "create measurement, insert point "

#create first admin user
$r = Invoke-WebRequest -Method Post -Uri "http://localhost:8086/query" -Body "q=CREATE USER $($cred.User) WITH PASSWORD '$($cred.Password)' WITH ALL PRIVILEGES" -UseBasicParsing -ContentType $contentType
if ($r.StatusCode -ne 200) {
    throw "Unable to create User"
}
write-output "create user"

$proc.Kill()
  

write-output "enable auth in config"
(get-content $influxconf | foreach-object {$_ -replace "auth-enabled = false" , "auth-enabled = true" })  | Set-Content $influxconf	

write-output "start influxdb with auth enabled"
$proc = Start-Process -FilePath $influxd -ArgumentList "-config $influxconf" -PassThru
Start-Sleep -s 5

$r = Invoke-WebRequest -Method Post -Uri "http://localhost:8086/query?" -Body "q=Show stats" -UseBasicParsing -ContentType $contentType -Headers $authHeader 
if ($r.StatusCode -ne 200) {
    throw "Unable to use new password"
}
write-output "done testing new UTF-8 password Auth header $($authHeader.Values)" 
