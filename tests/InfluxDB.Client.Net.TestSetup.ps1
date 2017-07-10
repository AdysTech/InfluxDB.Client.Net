##################################################################################################
#This script downloads the latest nightly InfluxDB build, starts the db engine, and creates basic
#data structures so that 'show databases' query will not fail during tests if the query gets 
#executed before CreateDatabase test
#
##################################################################################################
	
    $nightlies = $true
    if($nightlies -ne $true){
        $archive = if($IsLinux) { "influxdb-1.2.4_linux_amd64.tar.gz"} else {"influxdb-1.2.4_windows_amd64.zip"}        
	    $source = "https://dl.influxdata.com/influxdb/releases/$archive"
	} else {
        $archive = if($IsLinux) { "influxdb-nightly_linux_amd64.tar.gz"} else {"influxdb-nightly_windows_amd64.zip"}
	    $source = "https://dl.influxdata.com/influxdb/nightlies/$archive"
    }
    

    $archivepath = "$HOME/$archive"
	$influx = "$HOME/influxdb"
	$influxdata = "$HOME/.influxdb"
	
	if(!(test-path $archivepath) -or ((Get-ItemProperty -Path $archivepath -Name LastWriteTime).lastwritetime -lt $(get-date).AddDays(-1)))
	{	Invoke-WebRequest $source -OutFile $archivepath }
	
	if(test-path $influx)
	{	remove-item -recurse $influx -Force}

	#dependent on https://github.com/PowerShell/PowerShell/issues/2740
	if($IsLinux){
		mkdir $influx
		tar xf $archivepath -C $influx
		$influxd =  find $influx | grep bin/influxd
        $influxconf =  find $influx | grep bin/influxdb.conf
	} else {
		Expand-Archive -LiteralPath $archivepath -DestinationPath $influx -Force
		$influxd = Get-ChildItem $influx -File -filter "influxd.exe" -Recurse | % { $_.FullName }
        $influxconf = Get-ChildItem $influx -File -filter "influxdb.conf" -Recurse | % { $_.FullName }
        
	}
    

    #$x = 7z e $archivepath -o"$env:Temp/influxdb" -y
	if(test-path $influxdata)
	{ remove-item -recurse $influxdata }


    $proc = Start-Process -FilePath $influxd -PassThru


	$r = Invoke-WebRequest -Method Post -Uri "http://localhost:8086/query?" -Body "q=CREATE DATABASE prereq" -UseBasicParsing
	if($r.StatusCode -ne 200)
	{
		throw "Unable to create DB"
	}
	
	$r = Invoke-WebRequest -Method Post -Uri "http://localhost:8086/write?db=prereq" -Body "test,TestDate=$($(get-date).ToString('yyyyMMdd')),TestTime=$($(get-date).ToString('hhmm')) value=1234" -UseBasicParsing
	if($r.StatusCode -ne 204)
	{
		throw "Unable to create Measurement"
	}



    $user = "admin"
    $passwd = "admin@1nflux"
    $new_passwd = "test123€€€üöä§"

    #create first admin user
    $r = Invoke-WebRequest -Method Post -Uri "http://localhost:8086/query" -Body "q=CREATE USER $user WITH PASSWORD '$passwd' WITH ALL PRIVILEGES" -UseBasicParsing
	if($r.StatusCode -ne 200)
	{
		throw "Unable to create User"
	}

    $proc.Kill()
  
    #regenerate config
    Start-Process -FilePath $influxd -ArgumentList "config $influxconf" -RedirectStandardOutput $influxconf | Out-Null
    Start-Sleep -s 1

    #enable auth in config
    (get-content $influxconf | foreach-object {$_ -replace "auth-enabled = false" , "auth-enabled = true" })  | Set-Content $influxconf	

 	#start influxdb with auth enabled
    Start-Process -FilePath $influxd -ArgumentList "-config $influxconf" | Out-Null

	#let the engine stabilize
	Start-Sleep -s 10

	$r = Invoke-WebRequest -Method Post -Uri "http://localhost:8086/query?u=$user&p=$passwd" -Body "q=Show stats" -UseBasicParsing
	$r = Invoke-WebRequest -Method Post -Uri "http://localhost:8086/query?u=$user&p=$passwd" -Body "q=Select * from runtime limit 10&db=_internal" -UseBasicParsing -ContentType "application/x-www-form-urlencoded; charset=utf-8"

    $r = Invoke-WebRequest -Method Post -Uri "http://localhost:8086/query?u=$user&p=$passwd" -Body "q=SET PASSWORD FOR $user = '$new_passwd'" -UseBasicParsing -ContentType "application/x-www-form-urlencoded; charset=utf-8"
	if($r.StatusCode -ne 200)
	{
		throw "Unable to chnage password"
	}

    $r = Invoke-WebRequest -Method Post -Uri "http://localhost:8086/query?u=$user&p=$new_passwd" -Body "q=Show stats" -UseBasicParsing -ContentType "application/x-www-form-urlencoded; charset=utf-8"
    if($r.StatusCode -ne 200)
	{
		throw "Unable to use new password"
	}