##################################################################################################
#This script downloads the latest nightly InfluxDB build, starts the db engine, and creates basic
#data structures so that 'show databases' query will not fail during tests if the query gets 
#executed before CreateDatabase test
#
##################################################################################################
	$archive = if($IsLinux) { "influxdb-nightly_linux_amd64.tar.gz"} else {"influxdb-nightly_windows_amd64.zip"}
	$source = "https://dl.influxdata.com/influxdb/nightlies/$archive"
	$archivepath = "$HOME/$archive"
	$influx = "$HOME/influxdb"
	$influxdata = "$HOME/.influxdb"
	
	if(!(test-path $archivepath) -or ((Get-ItemProperty -Path $archivepath -Name LastWriteTime).lastwritetime -lt $(get-date).AddDays(-1)))
	{	Invoke-WebRequest $source -OutFile $archivepath }
	
	if(test-path $influx)
	{	remove-item -recurse $influx}

	#dependent on https://github.com/PowerShell/PowerShell/issues/2740
	if($IsLinux){
		mkdir $influx
		tar xf $archivepath -C $influx
		$influxd =  find $influx | grep bin/influxd
	} else {
		Expand-Archive -LiteralPath $archivepath -DestinationPath $influx -Force
		$influxd = Get-ChildItem $influx -File -filter "influxd.exe" -Recurse | % { $_.FullName }
	}
	
    #$x = 7z e $archivepath -o"$env:Temp/influxdb" -y
	if(test-path $influxdata)
	{ remove-item -recurse $influxdata }
	
    Start-Process -FilePath $influxd | Out-Null

	#let the engine start
	Start-Sleep -s 10
	
	$r = Invoke-WebRequest -Method Post -Uri http://localhost:8086/query -Body "q=CREATE DATABASE prereq" 
	if($r.StatusCode -ne 200)
	{
		throw "Unable to create DB"
	}
	
	$r = Invoke-WebRequest -Method Post -Uri http://localhost:8086/write?db=prereq -Body "test,TestDate=$($(get-date).ToString('yyyyMMdd')),TestTime=$($(get-date).ToString('hhmm')) value=1234"
	if($r.StatusCode -ne 204)
	{
		throw "Unable to create Measurement"
	}

	#let the engine stabilize
	Start-Sleep -s 20

	$r = Invoke-WebRequest -Method Post -Uri http://localhost:8086/query -Body "q=Show stats"
	$r = Invoke-WebRequest -Method Post -Uri http://localhost:8086/query -Body "q=Select * from runtime limit 10&db=_internal"