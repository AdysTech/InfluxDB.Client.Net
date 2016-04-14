# InfluxDB.Client.Net
[InfluxDB](http://influxdb.com) is new awesome open source time series database. But there is no official .Net client model for it. This is a feature rich .Net client for InfluxDB. All methods are exposed as Async methods, so that the calling thread will not be blocked. 
It currently supports

1.	Connecting using credentials
2.	Querying all existing databases
3.	Creating new database
4.	Querying for the whole DB structure (hierarchical structure of all measurements, and fields)
5.	Writing single, multiple values to db
6.  Retention policy management
7.  Post data to specific retention policies

To be added are

a.	Query for all tags, their unique values
b.	Deleting data, currently drop queries can be fired to delete data

###Usage
####Creating Client
`InfluxDBClient client = new InfluxDBClient (influxUrl, dbUName, dbpwd);`

####Querying all DB names
`List<String> dbNames = await client.GetInfluxDBNamesAsync ();`

####Querying DB structure
`Dictionary<string, List<String>> = await client.GetInfluxDBStructureAsync("<db name>");`
This returns a hierarchical structure of all measurements, and fields (but not tags)!

####Create new database
`CreateDatabaseAsync("<db name>");`
Creates a new database in InfluxDB. Does not raise exceptions if teh DB already exists. 


####Type safe data points
#####`InfluxDatapoint<T>`
It represents a single Point (collection of fields) in a series. In InfluxDB each point is uniquely identified by its series and timestamp.
Currently this class (as well as InfluxDB as of 0.9) supports `Boolean`, `String`, `Integer` and `Double` (additionally `decimal` is supported in client, which gets stored as a double in InfluxDB) types for field values. 
Multiple fields of same type are supported, as well as tags. 



####Writing data points to DB
    
    var valMixed = new InfluxDatapoint<InfluxValueField>();
    valMixed.UtcTimestamp = DateTime.UtcNow;
    valMixed.Tags.Add("TestDate", time.ToShortDateString());
    valMixed.Tags.Add("TestTime", time.ToShortTimeString());
    valMixed.Fields.Add("Doublefield", new InfluxValueField(rand.NextDouble()));
    valMixed.Fields.Add("Stringfield", new InfluxValueField(DataGen.RandomString()));
    valMixed.Fields.Add("Boolfield", new InfluxValueField(true));
    valMixed.Fields.Add("Int Field", new InfluxValueField(rand.Next()));
    
    valMixed.MeasurementName = measurementName;
    valMixed.Precision = TimePrecision.Seconds;
    valMixed.Retention = new InfluxRetentionPolicy() { Name = "Test2" };
    
    var r = await client.PostPointAsync(dbName, valMixed);

A collection of points can be posted using `await client.PostPointsAsync (dbName, points)`, where `points` can be collection of different types of `InfluxDatapoint`
	
####Query for data points

#####Single series

`await client.QueryDBAsync ("<db name>", "<query">);`

This function uses dynamic object (`ExpandoObject` to be exact), so `var r = await client.QueryDBAsync ("stress", "select * from performance limit 10");` will result in list of objects, where each object has properties with its value set to measument value.
So the result can be used like `r[0].time`. This also opens up a way to have an update mechanism as you can now query for data, change some values/tags etc, and write back. Since Influx uses combination of timestamp, tags as primary key, if you don't change tags, the values will be overwritten.

#####Multiple series

`await client.QueryMultiSeriesAsync("_internal", "SHOW STATS");`

This function returns `List<InfluxSeries>`, `InfluxSeries` is a custom object which will have a series name, set of tags (e.g. columns you used in `group by` clause. For the actual values, it will use dynamic object.
This allows to get data like `r.FirstOrDefault(x=>x.SeriesName=="queryExecutor").Entries[0].QueryDurationNs` kind of queries.
