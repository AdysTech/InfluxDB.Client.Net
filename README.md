# InfluxDB.Client.Net
[InfluxDB](http://influxdb.com) is new awesome open source time series database. But there is no official .Net client model for it. This is a feature rich .Net client for InfluxDB. All methods are exposed as Async methods, so that the calling thread will not be blocked. 
It currently supports

1.	Connecting using credentials
2.	Querying all existing databases
3.	Creating new database
4.	Querying for the whole DB structure (hierarchical structure of all measurements, and fields)
5.	Writing single, multiple values to db

To be added are

a.	Query for all tags, their unique values
b.	Updating retention policies
c.	Deleting data 

###Usage
####Creating Client
`InfluxDBClient client = new InfluxDBClient (influxUrl, dbUName, dbpwd);`

####Querying all DB names
`List<String> dbNames = await client.GetInfluxDBNamesAsync ();`

####Querying DB structure
`Dictionary<string, List<String>> = await client.GetInfluxDBStructureAsync("<db name>");`

####Create new database
`CreateDatabaseAsync("<db name>");`
