## v0.21.0 [08/25/2020]

### Release Notes
This version introduces methods to query and post strongly typed points using attributes. Thanks to @tituszban for adding the attribute support. Refer to [Readme.md](https://github.com/AdysTech/InfluxDB.Client.Net/blob/master/README.md) for examples. 


## v0.20.0 [08/11/2020]

### Release Notes
This version merges the .NET Framework and .NET Core projects into one multi-target project. Revert the .NET Standard 2.1 to .NET Standard 2.0.

### Bugfixes

- [#97](https://github.com/AdysTech/InfluxDB.Client.Net/issues/97): Switch to .NET Standard 2.1 prevent use from applications using .NET framework
- [#96](https://github.com/AdysTech/InfluxDB.Client.Net/issues/96): Improve error handling for 400  



## v0.15.0 [04/01/2020]

### Release Notes
Migrate to .net core 3.1. I have also moved the base .Net framework to 4.6 (long overdue). If there are anyone still using 4.5.1 I am sorry, its high time you upgrade. Bug & Performance fixes.

### Bugfixes

- [#90](https://github.com/AdysTech/InfluxDB.Client.Net/issues/90): PostPointsAsync method is so slow in linux os
- [#91](https://github.com/AdysTech/InfluxDB.Client.Net/issues/91): StatusCode Forbidden not considered in GetAsync()
- [#84](https://github.com/AdysTech/InfluxDB.Client.Net/issues/84): Execute Continuous Query every seconds 

## v0.11.0 [02/01/2020]

### Release Notes
Option to add a retention policy with infinite duration - Thanks to @jasase 
Handle truncated responses due to "max-row-limit", (flag "partial=true"). Thanks to @tbraun-hk 

### Bugfixes

- [#82](https://github.com/AdysTech/InfluxDB.Client.Net/issues/82): QueryMultiSeriesAsync should return "partial" if responses are truncated by InfluxDB
- [#83](https://github.com/AdysTech/InfluxDB.Client.Net/issues/83): Create Infinite Retention Policy

## v0.9.0 [10/12/2019]

### Release Notes
Add  MeasurementHierarchy to IInfluxDatabase, SeriesCount and PointsCount properties to IInfluxMeasurement
Now calling `GetInfluxDBStructureAsync` populates the structure with retention policies, and also gives the unique series and point counts for each of the measurements
Allow for deleting/dropping the Influx database by calling `DropDatabaseAsync`, allow delete entire measurement via	`DropMeasurementAsync` as well as specific data points with and where clause using `TestDeletePointsAsync`

### Bugfixes

- [#72](https://github.com/AdysTech/InfluxDB.Client.Net/issues/72): Performance improvements
- [#69](https://github.com/AdysTech/InfluxDB.Client.Net/issues/69): easiest way to count the entries in the measurement
- [#67](https://github.com/AdysTech/InfluxDB.Client.Net/issues/67): delete an existing database
- [#41](https://github.com/AdysTech/InfluxDB.Client.Net/issues/41): DELETE FROM {Measurement} WHERE

## v0.9.0 [10/12/2019]

### Release Notes
Add  MeasurementHierarchy to IInfluxDatabase, SeriesCount and PointsCount properties to IInfluxMeasurement
Now calling `GetInfluxDBStructureAsync` populates the structure with retention policies, and also gives the unique series and point counts for each of the measurements
Allow for deleting/dropping the Influx database by calling `DropDatabaseAsync`, allow delete entire measurement via	`DropMeasurementAsync` as well as specific data points with and where clause using `TestDeletePointsAsync`

### Bugfixes

- [#72](https://github.com/AdysTech/InfluxDB.Client.Net/issues/72): Performance improvements
- [#69](https://github.com/AdysTech/InfluxDB.Client.Net/issues/69): easiest way to count the entries in the measurement
- [#67](https://github.com/AdysTech/InfluxDB.Client.Net/issues/67): delete an existing database
- [#41](https://github.com/AdysTech/InfluxDB.Client.Net/issues/41): DELETE FROM {Measurement} WHERE

## v0.8.0 [3/14/2019]

### Release Notes
Requires now .Net Standard 2.0 instead of 1.6. New Interface IInfluxValueField and extended support of basic types in InfluxValueField when `InfluxDbClient.PostPointsAsync`.

## v0.7.3 [3/14/2019]

### Release Notes
Add batch size variable in InfluxDbClient.PostPointsAsync method

### Bugfixes

- [#59](https://github.com/AdysTech/InfluxDB.Client.Net/issues/59): Troubles with batch inserting

## v0.7.2 [2/7/2019]

### Release Notes
Handle InfluxUrl with paths

### Bugfixes

- [#57](https://github.com/AdysTech/InfluxDB.Client.Net/issues/57): API does not support URLs including a path 

## v0.7.1 [1/5/2018]

### Release Notes
Handle trailing slash in InfluxUrl

### Bugfixes

- [#54](https://github.com/AdysTech/InfluxDB.Client.Net/issues/54): InfluxUrl with trailing slash "/" result in error 

## v0.7.0 [12/31/2018]

### Release Notes
Fix posting to autogen (default) retention policies.

### Bugfixes

- [#52](https://github.com/AdysTech/InfluxDB.Client.Net/issues/52): PostPointsAsync does not post any points if AutoGen retention policy is used

## v0.6.9 [10/10/2018]

### Release Notes
Fix escape sequence to match InfluxDB documentation

### Bugfixes

- [#42](https://github.com/AdysTech/InfluxDB.Client.Net/issues/42): Stop changing column header case to Camel case
- [#48](https://github.com/AdysTech/InfluxDB.Client.Net/issues/48): Incorrect escape sequence for string values with space
- [#50](https://github.com/AdysTech/InfluxDB.Client.Net/issues/50): Missing quotes for retention policy name

## v0.6.8 [7/18/2018]

### Release Notes
Add support for communicating via Proxy server. Update Basic Authentication to UTF-8. Thanks to @hadamarda for adding it in #43

## v0.6.7 [7/09/2017]

### Release Notes
Allow non ASCII passwords, escape double quotes (Thanks to [@siavelis](https://github.com/siavelis) for #37) 

### Bugfixes

- [#38](https://github.com/AdysTech/InfluxDB.Client.Net/issues/38): Authentication attempts fail if password contains non-ASCII chars
- [#36](https://github.com/AdysTech/InfluxDB.Client.Net/issues/36): PostPointAsync() fails with fields containing double quotes

## v0.6.1 [6/03/2017]

### Release Notes
Allow points to be passed without explicitly setting time or precision. It also fixes an issue with previous implementation of the chunking support.


### Bugfixes

- [#31](https://github.com/AdysTech/InfluxDB.Client.Net/issues/31): IndexOutOfRangeException thrown for partial writes

### Features

- [#30](https://github.com/AdysTech/InfluxDB.Client.Net/issues/30): Use NanoSeconds as default precision


### Breaking Change
- Library will silently drop points older than retention period. This is similar to InfluDB behavior where it will reject those points with an `{"error":"partial write: points beyond retention policy dropped=225"}`


## v0.6.1 [3/28/2017]

### Release Notes

This version adds Chunking support available in InfluxDB. Currently there is a open [issue #8212](https://github.com/influxdata/influxdb/issues/8212) on Influxdb side, which is been handled with code, which needs to be removed once the issue is fixed.

### Bugfixes

- [#28](https://github.com/AdysTech/InfluxDB.Client.Net/issues/28): Service Unavailable exception not thrown for unknown host

### Features

- [#23](https://github.com/AdysTech/InfluxDB.Client.Net/issues/23): Chunking Support 


### Breaking Change
- `QueryDBAsync` which was deprecated has been removed. Please use `QueryMultiSeriesAsync` instead.


## v0.6.0 [3/14/2017]

### Release Notes
This version adds support for .Net Core.  

### Features

- [#17](https://github.com/AdysTech/InfluxDB.Client.Net/issues/17): Net Core Support
- Travis CI integration

## v0.5.9.1 [10/29/2016]
Minor release to fix a blocking bug with data series with microsecond precision.

### Breaking Change
- `InfluxDBClient` now implements `IDisposable`, so plan to expect few warnings in your code. Similarly custom exceptions from this library are marked as `Serializable`

### Bugfixes

- [#21](https://github.com/AdysTech/InfluxDB.Client.Net/issues/21): Microsecond precision data is stored as epoch 0


## v0.5.9 [10/25/2016]

### Release Notes
This version is a precursor to enable support for .Net Core.  This restructures the solution folders, makes the unit tests truly independent of each other. It also supports deleting Continuous Queries. As of now it still does not support the .NBet Core as there are few open issues (e.g. [#3199](https://github.com/dotnet/cli/issues/3199), [#147](https://github.com/aspnet/Tooling/issues/147)) which are rooted in .Net Core's dependency on `xproj` model. Once they move to `csproj` these issues will be resolved, and this library will support .Net Core as well via .Net Standard (1.6+) PCL. Final goal is to have same set of source files, which gets complied as normal .Net 4.5 class library, and a PCL for the Core.

This version moves away from `JavaScriptSerializer` to [Json.NET](https://github.com/JamesNK/Newtonsoft.Json). Functionally this is a transparent change. But this is required as former is not supported in .Net Core.

### Features

- [#16](https://github.com/AdysTech/InfluxDB.Client.Net/issues/16): Create / Delete Continuous Query
- QueryMultiSeriesAsync returns the time in a `DateTime` object if the series has one. The method also supports different epoch precisions in query results now.


### Breaking Change
- `QueryDBAsync` has been deprecated and will be removed in next version. Please use `QueryMultiSeriesAsync` instead.

## v0.5.5 [9/25/2016]

### Release Notes
This version supports Continuous Queries. You can create a new CQ, or as well as query them. This version also has generic code changes to use new (well .Net 4.6 feature, so quite old) string interpolation syntax to remove `String.Format("string {0}", interolatedValue)` instead use `"$string{interolatedValue}"`.
Another change is to throw `InfluxDBException` for any query failures. Before there were few cases where `ArgumentException` was thrown.

### Features

- [#16](https://github.com/AdysTech/InfluxDB.Client.Net/issues/16): Create Continuous Query


## v0.5.3 [9/15/2016]
### Bugfixes

- [#14](https://github.com/AdysTech/InfluxDB.Client.Net/issues/14): Regex for error extraction fails with type conflict errors. Thanks to @patHyatt for reporting and fixing it in #15


## v0.5.3 [7/8/2016]

### Release Notes
Updated to handle default auto generated retention policy name other than "default" [#6586](https://github.com/influxdata/influxdb/pull/6586)

## v0.5.1 [7/2/2016]

### Release Notes
This is a minor update to revert to using .Net 4.5.1. Previous version was changed to use .Net 4.6 by mistake.

## v0.5.0 [4/15/2016]

### Release Notes
This version supports Retention policies. You can create a new retention policy, or post into existing ones, as well as query them. This version also supports querying multi series data. 
There is a breaking change to `GetInfluxDBStructureAsync`, which now returns a custom InfluxDB object, which not only provides info about measurements, but also about tags and fields in each of them.

### Features

- [#8](https://github.com/AdysTech/InfluxDB.Client.Net/pull/8): Add Nuget Package
- [#10](https://github.com/AdysTech/InfluxDB.Client.Net/pull/10): Support for Retention Policy, query Influx Version
- [#11](https://github.com/AdysTech/InfluxDB.Client.Net/pull/11): Enable AppVeyor integration for CI tests

### Bugfixes

- [#7](https://github.com/AdysTech/InfluxDB.Client.Net/issues/7): InfluxDataPoint fields cannot be different types. 
- [#9](https://github.com/AdysTech/InfluxDB.Client.Net/issues/9): confused by "Integer" support 

## v0.2.1.0 [12/31/2015]

### Release Notes
Added type safe write methods via a new data structure called InfluxDataPoint. Previous write methods are marked obsolete. 

### Features

- [#5](https://github.com/AdysTech/InfluxDB.Client.Net/pull/5): Added InfluxDataPoint, and PostPoint methods 
- [#6](https://github.com/AdysTech/InfluxDB.Client.Net/pull/6): Added Partial Writes, and malformed requests 


## v0.1.1.0 [12/19/2015]

### Release Notes
Added the functionality to query for existing data from InfluxDB. Also unknown little quirk was Influx's need for . (dot) to treat a number as a number, so non US local code can beak Influx data writes. , now double to string conversion will work in any locale.

### Features

- [#4](https://github.com/AdysTech/InfluxDB.Client.Net/pull/4): Add Querying InfluxDB functionality 

### Bugfixes

- [#3](https://github.com/AdysTech/InfluxDB.Client.Net/pull/3): Double to str conversion fix, Thanks to @spamik
