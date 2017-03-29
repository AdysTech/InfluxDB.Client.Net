#### Build Status
1. AppVeyor [![Build status](https://ci.appveyor.com/api/projects/status/ryj9u8dpcasu1xur?svg=true)](https://ci.appveyor.com/project/AdysTech/influxdb-client-net)
2. Travis CI [![Build status](https://travis-ci.org/AdysTech/InfluxDB.Client.Net.svg?branch=master)](https://travis-ci.org/AdysTech/InfluxDB.Client.Net.svg?branch=master)

**Now supports .Net Core, run same .Net code in Windows and Linux**

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
8.  Query for all Continuous Queries
9.  Create a new Continuous Query
10. Drop continuous queries
11. Chunked queries

To be added are

1.	Query for all tags, their unique values
2.	Deleting data, currently drop queries can be fired to delete data

####  Nuget
1. .Net 4.5.1 > [AdysTech.InfluxDB.Client.Net](https://www.nuget.org/packages/AdysTech.InfluxDB.Client.Net)
2. .Core 1.1 > [AdysTech.InfluxDB.Client.Net.Core](https://www.nuget.org/packages/AdysTech.InfluxDB.Client.Net.Core/)

### Usage
#### Creating Client
`InfluxDBClient client = new InfluxDBClient (influxUrl, dbUName, dbpwd);`

#### Querying all DB names
`List<String> dbNames = await client.GetInfluxDBNamesAsync ();`

#### Querying DB structure
`Dictionary<string, List<String>> = await client.GetInfluxDBStructureAsync("<db name>");`
This returns a hierarchical structure of all measurements, and fields (but not tags)!

#### Create new database
`CreateDatabaseAsync("<db name>");`
Creates a new database in InfluxDB. Does not raise exceptions if the DB already exists. 


#### Type safe data points
##### `InfluxDatapoint<T>`
It represents a single Point (collection of fields) in a series. In InfluxDB each point is uniquely identified by its series and timestamps.
Currently this class (as well as InfluxDB as of 0.9) supports `Boolean`, `String`, `Integer` and `Double` (additionally `decimal` is supported in client, which gets stored as a double in InfluxDB) types for field values. 
Multiple fields of same type are supported, as well as tags. 



#### Writing data points to DB
    
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
	
#### Query for data points

    var r = await client.QueryMultiSeriesAsync ("_internal", "select * from runtime limit 10");
    var s = await client.QueryMultiSeriesAsync("_internal", "SHOW STATS");
    var rc = await client.QueryMultiSeriesAsync ("_internal", "select * from runtime limit 100",10);

`QueryMultiSeriesAsync` method returns `List<InfluxSeries>`, `InfluxSeries` is a custom object which will have a series name, set of tags (e.g. columns you used in `group by` clause. For the actual values, it will use dynamic object(`ExpandoObject` to be exact). The example #1 above will result in a single entry list, and the result can be used like `r.FirstOrDefault()?.Entries[0].time`. This also opens up a way to have an update mechanism as you can now query for data, change some values/tags etc, and write back. Since Influx uses combination of timestamp, tags as primary key, if you don't change tags, the values will be overwritten.

Second example above will provide multiple series objects, and allows to get data like `r.FirstOrDefault(x=>x.SeriesName=="queryExecutor").Entries[0].QueryDurationNs`.

The last example above makes InfluxDB to split the selected points (100 limited by `limit` clause) to multiple series, each having 10 points as given by `chunk` size.

#### Retention Policies
This library uses a cutsom .Net object to represent the Influx Retention Policy. The `Duration` concept is nicely wraped in `TimeSpan`, so it can be easily manipulated using .Net code. It also supports `ShardDuration` concept introduced in recent versions of InfluxDB.

##### Get all Retention Policies from DB
`var policies = await client.GetRetentionPoliciesAsync (dbName);`

##### Create Retention Policy and Write points to a specific retention policy
The code below will create a new retention policy with a retention period of 6 hours, and write a point to that policy.
 
    var rp = new InfluxRetentionPolicy () { Name = "Test2", DBName = dbName, Duration = TimeSpan.FromHours (6), IsDefault = false };
    if (!await client.CreateRetentionPolicyAsync (rp))
    	throw new InvalidOperationException ("Unable to create Retention Policy");
              
    valMixed.MeasurementName = measurementName;
    valMixed.Precision = TimePrecision.Seconds;
    valMixed.Retention = new InfluxRetentionPolicy () { Name = "Test2"}; //or you can just assign this to rp   
    var r = await client.PostPointAsync (dbName, valMixed);


#### Continuous Queries
Similar to retention policy the library also creates a custom object to represent the `Continuous Queries`. Because the queries can be very complex, the actual query part of the CQ is exposed as just a string. But the timing part of [CQ](https://docs.influxdata.com/influxdb/v1.0/query_language/continuous_queries) specifically <intervals> given in `[RESAMPLE [EVERY <interval>] [FOR <interval>]]` are exposed as `TimeSpan` objects. Since the `GROUP BY time(<interval>)` is mandated for CQs this interval again is exposed as `TimeSpan`. This allows you to run LINQ code on collection of CQs.

##### Get all Continuous Queries present in DB
`var cqList = await client.GetContinuousQueriesAsync ();`

##### Create a new Continuous Query
    var p = new InfluxContinuousQuery () { Name = "TestCQ1",
                                DBName = dbName,
                                Query = "select mean(Intfield) as Intfield into cqMeasurement from testMeasurement group by time(1h),*",
                                ResampleDuration = TimeSpan.FromHours (2),
                                ResampleFrequency = TimeSpan.FromHours (0.5) };
    var r = await client.CreateContinuousQueryAsync (p);

##### Drop a Continuous Query
` var r = await client.DropContinuousQueryAsync (p);`
`p` has to be an existing CQ, which is already saved. 
