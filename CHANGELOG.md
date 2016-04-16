## v0.5.0 [4/15/2016]

### Release Notes
This version supports Retention policies. You can create a new retention policy, or post into existing ones, as well as query them. This version also supports querying multi series data. 
There is a breaking change to `GetInfluxDBStructureAsync`, which now returns a custom InfluxDB object, which not only provides info about measurements, but also about tags and fields in each of them.

### Features

- [#8](https://github.com/AdysTech/InfluxDB.Client.Net/pull/8): Add Nuget Package
- [#10](https://github.com/AdysTech/InfluxDB.Client.Net/pull/10): Support for Retention Policy, query Influx Version
- [#11](https://github.com/AdysTech/InfluxDB.Client.Net/pull/11): Enable AppVeyor integration for ci tests

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