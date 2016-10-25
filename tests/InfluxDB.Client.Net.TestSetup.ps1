##################################################################################################
#This script downloads the latest nightly InfluxDB build, starts the db engine, and creates basic
#data structures so that 'show databases' query will not fail during tests if the query gets 
#executed before CreateDatabase test
#
##################################################################################################

	$source = "https://dl.influxdata.com/influxdb/nightlies/influxdb-nightly_windows_amd64.zip"
	$destination = "$env:Temp\influxdb-nightly_windows_amd64.zip"
	$influx = "$env:Temp\influxdb"
	$influxdata = "$env:UserProfile\.influxdb"
	
	if(!(test-path $destination) -or ((Get-ItemProperty -Path $destination -Name LastWriteTime).lastwritetime -lt $(get-date).AddDays(-1)))
	{	Invoke-WebRequest $source -OutFile $destination }
	
	if(test-path $influx)
	{	rmdir -recurse $influx}

	Add-Type -As System.IO.Compression.FileSystem
	[System.IO.Compression.ZipFile]::ExtractToDirectory($destination,$influx)
	$influxd = Get-ChildItem $influx -File -filter "influxd.exe" -Recurse | % { $_.FullName }
	
    #$x = 7z e $destination -o"$env:Temp\influxdb" -y
	if(test-path $influxdata)
	{ rmdir -recurse "$env:UserProfile\.influxdb" }
	
    Start-Process -FilePath $influxd
	#let the engine start
	Start-Sleep -s 10
	
	$r = Invoke-WebRequest -Method Post -Uri http://localhost:8086/query -Body "q=CREATE DATABASE prereq" 
	if($r.StatusCode -ne 200)
	{
		throw "Unable to create DB"
	}
	
	$r = Invoke-WebRequest -Method Post -Uri http://localhost:8086/write?db=prereq -Body "test,date=$($(get-date).ToString('yyyyMMdd')),time=$($(get-date).ToString('hhmm')) value=1234"
	if($r.StatusCode -ne 204)
	{
		throw "Unable to create Measurement"
	}

	#let the engine stabilize
	Start-Sleep -s 20

	$r = Invoke-WebRequest -Method Post -Uri http://localhost:8086/query -Body "q=Show stats"
	$r = Invoke-WebRequest -Method Post -Uri http://localhost:8086/query -Body "q=Select * from runtime limit 10&db=_internal"