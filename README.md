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

###--------------New Version - 0.1.1.0 - 12/19/2015--------------------
Added the functionality to query for existing data from InfluxDB

####Query for data points
`await client.QueryDBAsync ("<db name>", "<query">);`

This function uses dynamic object (`ExpandoObject` to be exact), so `var r = await client.QueryDBAsync ("stress", "select * from performance limit 10");` will result in list of objects, where each object has properties with its value set to measument value.
So the result can be used like `r[0].time`. This also opens up a way to have an update mechanism as you can now query for data, change some values/tags etc, and write back. Since Influx uses combination of timestamp, tags as primary key, if you don't change tags, the values will be overwritten.

Also unknown little quirk was Influx's need for . (dot) to treat a number as a number, so non US local code can beak Influx data writes. Thanks to @spamik, now double to string conversion will work in any locale.
