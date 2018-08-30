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
Allow points to be passed without explictly setting time or precision. It also fixes an issue with previopus implementation of the chunking support.


### Bugfixes

- [#31](https://github.com/AdysTech/InfluxDB.Client.Net/issues/31): IndexOutOfRangeException thrown for partial writes

### Features

- [#30](https://github.com/AdysTech/InfluxDB.Client.Net/issues/30): Use NanoSeconds as default precision


### Breaking Change
- Library will silently drop points older than retention period. This is similar to InfluDB behavios where it will reject those points with an `{"error":"partial write: points beyond retention policy dropped=225"}`


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
- `InfluxDBClient` now implements `IDispobale`, so plan to expect few warnings in your code. Similarly custom exceptions from this library are marked as `Serializable`

### Bugfixes

- [#21](https://github.com/AdysTech/InfluxDB.Client.Net/issues/21): Microsecond precision data is stored as epoch 0


## v0.5.9 [10/25/2016]

### Release Notes
This version is a precursor to enable support for .Net Core.  This restructures the solution folders, makes the unit tests truly independent of each other. It also supports deleting Continuous Queries. As of now it still does not support the .NBet Core as there are few open issues (e.g. [#3199](https://github.com/dotnet/cli/issues/3199), [#147](https://github.com/aspnet/Tooling/issues/147)) which are rooted in .Net Core's dependency on `xproj` model. Once they move to `csproj` these issues will be resolved, and this library will support .Net Core as well via .Net Standard (1.6+) PCL. Final goal is to have same set of source files, which gets complied as normal .Net 4.5 class library, and a PCL for the Core.

This version moves away from `JavaScriptSerializer` to [Json.NET](https://github.com/JamesNK/Newtonsoft.Json). Functionally this is a transparent chnage. But this is required as former is not supported in .Net Core.

### Features

- [#16](https://github.com/AdysTech/InfluxDB.Client.Net/issues/16): Create / Delete Continuous Query
- QueryMultiSeriesAsync returns the time in a `DateTime` object if the series has one. The method also supports different epoch precisoins in query results now.


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