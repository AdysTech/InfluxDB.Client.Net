using AdysTech.InfluxDB.Client.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AdysTech.InfluxDB.Client.Net.Tests
{
    [TestClass()]
    public class InfluxDBClientTest
    {

        const string dbName = "testDB";
        const string internalDb = "_internal";
        const string measurementName = "TestMeasurement";
        const string invalidDbName = @"test \DB";
        const string dbUName = "admin";
        const string dbpwd = "test123$€₹#₳₷ȅ";
        const string invalidUName = "invalid";
        const string influxUrl = "http://localhost:8086";
        const string invalidInfluxUrl = "http://xyzerty:8089";


        [TestMethod, TestCategory("Get")]
        public async Task TestGetInfluxDBNamesAsync_ServiceUnavailable()
        {
            var client = new InfluxDBClient(invalidInfluxUrl);
            var r = await AssertEx.ThrowsAsync<ServiceUnavailableException>(() => client.GetInfluxDBNamesAsync());
        }

        [TestMethod, TestCategory("Get")]
        public async Task TestGetInfluxDBNamesAsync()
        {
            try
            {
                var client = new InfluxDBClient(influxUrl, dbUName, dbpwd);
                Stopwatch s = new Stopwatch();
                s.Start();
                var r = await client.GetInfluxDBNamesAsync();
                s.Stop();
                Debug.WriteLine(s.ElapsedMilliseconds);

                Assert.IsTrue(r != null, "GetInfluxDBNamesAsync retunred null or empty collection");
            }

            catch (Exception e)
            {
                Assert.Fail($"Unexpected exception of type {e.GetType()} caught: {e.Message}");
                return;
            }
        }

        [TestMethod, TestCategory("Create")]
        public async Task TestCreateDatabaseAsync_InvalidName()
        {
            var client = new InfluxDBClient(influxUrl, dbUName, dbpwd);
            var r = await AssertEx.ThrowsAsync<InfluxDBException>(() => client.CreateDatabaseAsync(invalidDbName));
        }

        [TestMethod, TestCategory("Create")]
        public async Task TestCreateDatabaseAsync()
        {
            try
            {
                var client = new InfluxDBClient(influxUrl, dbUName, dbpwd);
                var r = await client.CreateDatabaseAsync(dbName);
                Assert.IsTrue(r, "CreateDatabaseAsync retunred false");
            }
            catch (InvalidOperationException e)
            {
                Assert.IsTrue(e.Message == "database already exists");
            }
            catch (Exception e)
            {
                Assert.Fail($"Unexpected exception of type {e.GetType()} caught: {e.Message}");
                return;
            }
        }

        [TestMethod, TestCategory("Get")]
        public async Task TestGetInfluxDBStructureAsync_InvalidDB()
        {
            try
            {
                var client = new InfluxDBClient(influxUrl, dbUName, dbpwd);
                var r = await client.GetInfluxDBStructureAsync("InvalidDB");
                Assert.IsTrue(r != null && r.Measurements.Count() == 0, "GetInfluxDBStructureAsync retunred non null or non empty collection");
            }
            catch (Exception e)
            {
                Assert.Fail($"Unexpected exception of type {e.GetType()} caught: {e.Message}");
                return;
            }
        }

        [TestMethod, TestCategory("Get")]
        public async Task TestGetInfluxDBStructureAsync()
        {
            try
            {
                var client = new InfluxDBClient(influxUrl, dbUName, dbpwd);
                Stopwatch s = new Stopwatch();
                s.Start();
                var r = await client.GetInfluxDBStructureAsync(dbName);
                s.Stop();
                Debug.WriteLine(s.ElapsedMilliseconds);
                Assert.IsTrue(r != null && r.Measurements.Count >= 0, "GetInfluxDBStructureAsync retunred null or non empty collection");
            }
            catch (Exception e)
            {
                Assert.Fail($"Unexpected exception of type {e.GetType()} caught: {e.Message}");
                return;
            }
        }


        [TestMethod, TestCategory("Query")]
        public async Task TestQueryMultiSeriesAsync()
        {
            var client = new InfluxDBClient(influxUrl, dbUName, dbpwd);

            Stopwatch s = new Stopwatch();
            s.Start();
            var r = await client.QueryMultiSeriesAsync(dbName, "SHOW STATS");

            s.Stop();
            Debug.WriteLine($"Elapsed{s.ElapsedMilliseconds}");
            Assert.IsTrue(r != null, "QueryMultiSeriesAsync retunred null or invalid data");
        }



        [TestMethod, TestCategory("Post")]
        public async Task TestPostPointsAsync()
        {
            try
            {
                var client = new InfluxDBClient(influxUrl, dbUName, dbpwd);
                var time = DateTime.Now;
                var today = DateTime.Now.ToShortDateString();
                var now = DateTime.Now.ToShortTimeString();

                var points = new List<IInfluxDatapoint>();

                var valDouble = new InfluxDatapoint<double>();
                valDouble.UtcTimestamp = DateTime.UtcNow;
                valDouble.Tags.Add("TestDate", today);
                valDouble.Tags.Add("TestTime", now);
                valDouble.Fields.Add("Doublefield", DataGen.RandomDouble());
                valDouble.Fields.Add("Doublefield2", DataGen.RandomDouble());
                valDouble.MeasurementName = measurementName;
                valDouble.Precision = TimePrecision.Nanoseconds;
                points.Add(valDouble);

                valDouble = new InfluxDatapoint<double>();
                valDouble.UtcTimestamp = DateTime.UtcNow;
                valDouble.Tags.Add("TestDate", today);
                valDouble.Tags.Add("TestTime", now);
                valDouble.Fields.Add("Doublefield", DataGen.RandomDouble());
                valDouble.Fields.Add("Doublefield2", DataGen.RandomDouble());
                valDouble.MeasurementName = measurementName;
                valDouble.Precision = TimePrecision.Microseconds;
                points.Add(valDouble);

                var valInt = new InfluxDatapoint<int>();
                valInt.UtcTimestamp = DateTime.UtcNow;
                valInt.Tags.Add("TestDate", today);
                valInt.Tags.Add("TestTime", now);
                valInt.Fields.Add("Intfield", DataGen.RandomInt());
                valInt.Fields.Add("Intfield2", DataGen.RandomInt());
                valInt.MeasurementName = measurementName;
                valInt.Precision = TimePrecision.Milliseconds;
                points.Add(valInt);

                valInt = new InfluxDatapoint<int>();
                valInt.UtcTimestamp = DateTime.UtcNow;
                valInt.Tags.Add("TestDate", today);
                valInt.Tags.Add("TestTime", now);
                valInt.Fields.Add("Intfield", DataGen.RandomInt());
                valInt.Fields.Add("Intfield2", DataGen.RandomInt());
                valInt.MeasurementName = measurementName;
                valInt.Precision = TimePrecision.Seconds;
                points.Add(valInt);

                var valBool = new InfluxDatapoint<bool>();
                valBool.UtcTimestamp = DateTime.UtcNow;
                valBool.Tags.Add("TestDate", today);
                valBool.Tags.Add("TestTime", now);
                valBool.Fields.Add("Booleanfield", time.Ticks % 2 == 0);
                valBool.MeasurementName = measurementName;
                valBool.Precision = TimePrecision.Minutes;
                points.Add(valBool);

                var valString = new InfluxDatapoint<string>();
                valString.UtcTimestamp = DateTime.UtcNow;
                valString.Tags.Add("TestDate", today);
                valString.Tags.Add("TestTime", now);
                valString.Fields.Add("Stringfield", DataGen.RandomString());
                valString.MeasurementName = measurementName;
                valString.Precision = TimePrecision.Hours;
                points.Add(valString);


                var r = await client.PostPointsAsync(dbName, points);
                Assert.IsTrue(r, "PostPointsAsync retunred false");
            }
            catch (Exception e)
            {

                Assert.Fail($"Unexpected exception of type {e.GetType()} caught: {e.Message}");
                return;
            }
        }


        [TestMethod, TestCategory("Post")]
        public async Task TestPostPointAsync_InvalidReq()
        {
            var client = new InfluxDBClient(influxUrl, dbUName, dbpwd);
            var time = DateTime.Now;
            var today = DateTime.Now.ToShortDateString();
            var now = DateTime.Now.ToShortTimeString();

            var valDouble = new InfluxDatapoint<double>();
            valDouble.UtcTimestamp = DateTime.UtcNow;
            valDouble.Tags.Add("TestDate", today);
            valDouble.Tags.Add("TestTime", now);
            valDouble.Fields.Add("Doublefield", DataGen.RandomDouble());
            valDouble.Fields.Add("Doublefield2", Double.NaN);
            valDouble.MeasurementName = measurementName;
            valDouble.Precision = TimePrecision.Seconds;

            var r = await AssertEx.ThrowsAsync<InfluxDBException>(() => client.PostPointAsync(dbName, valDouble));

        }


        [TestMethod, TestCategory("Post")]
        public async Task TestPostPointsAsync_PartialWrite()
        {
            var client = new InfluxDBClient(influxUrl, dbUName, dbpwd);
            var time = DateTime.Now;
            var today = DateTime.Now.ToShortDateString();
            var now = DateTime.Now.ToShortTimeString();

            var points = new List<IInfluxDatapoint>();


            var valDouble = new InfluxDatapoint<double>();
            valDouble.UtcTimestamp = DateTime.UtcNow;
            valDouble.Tags.Add("TestDate", today);
            valDouble.Tags.Add("TestTime", now);
            valDouble.Fields.Add("Doublefield", DataGen.RandomDouble());
            valDouble.Fields.Add("Doublefield2", Double.NaN);
            valDouble.MeasurementName = measurementName;
            valDouble.Precision = TimePrecision.Seconds;
            points.Add(valDouble);


            for (int i = 0; i < 5; i++)
            {
                var valInt = new InfluxDatapoint<int>();
                valInt.UtcTimestamp = DateTime.UtcNow.AddSeconds(-1 * DataGen.RandomInt(3600));
                valInt.Tags.Add("TestDate", today);
                valInt.Tags.Add("TestTime", now);
                valInt.Fields.Add("Intfield", DataGen.RandomInt());
                valInt.Fields.Add("Intfield2", DataGen.RandomInt());
                valInt.MeasurementName = measurementName;
                valInt.Precision = TimePrecision.Seconds;
                points.Add(valInt);
            }

            valDouble = new InfluxDatapoint<double>();
            valDouble.UtcTimestamp = DateTime.UtcNow;
            valDouble.Tags.Add("TestDate", today);
            valDouble.Tags.Add("TestTime", now);
            valDouble.Fields.Add("Doublefield", DataGen.RandomDouble());
            valDouble.Fields.Add("Doublefield2", Double.NaN);
            valDouble.MeasurementName = measurementName;
            valDouble.Precision = TimePrecision.Seconds;
            points.Add(valDouble);


            var r = await AssertEx.ThrowsAsync<InfluxDBException>(() => client.PostPointsAsync(dbName, points));

        }

        /// <summary>
        /// This will create 2000 objects, and posts them to Influx. Since the precision is random, many points get overwritten 
        /// (e.g. you can have only point per hour at hour precision.
        /// </summary>
        /// <returns></returns>
        [TestMethod, TestCategory("Post")]
        public async Task TestPostPointsAsync_Batch()
        {
            try
            {
                var client = new InfluxDBClient(influxUrl, dbUName, dbpwd);

                var points = new List<IInfluxDatapoint>();

                var today = DateTime.Now.ToShortDateString();
                var now = DateTime.Now.ToShortTimeString();
                var start = DateTime.Now.AddDays(-5);
                var end = DateTime.Now;

                for (int i = 0; i < 5000; i++)
                {
                    var valMixed = new InfluxDatapoint<InfluxValueField>();
                    valMixed.UtcTimestamp = DataGen.RandomDate(start, end);
                    valMixed.Tags.Add("TestDate", today);
                    valMixed.Tags.Add("TestTime", now);
                    valMixed.UtcTimestamp = DateTime.UtcNow;
                    valMixed.Fields.Add("Doublefield", new InfluxValueField(DataGen.RandomDouble()));
                    valMixed.Fields.Add("Stringfield", new InfluxValueField(DataGen.RandomString()));
                    valMixed.Fields.Add("Boolfield", new InfluxValueField(DateTime.Now.Ticks % 2 == 0));
                    valMixed.Fields.Add("Int Field", new InfluxValueField(DataGen.RandomInt()));
                    valMixed.MeasurementName = measurementName;
                    valMixed.Precision = (TimePrecision)(DataGen.RandomInt() % 6) + 1;
                    points.Add(valMixed);
                }

                var r = await client.PostPointsAsync(dbName, points);
                Assert.IsTrue(points.TrueForAll(p => p.Saved == true), "PostPointsAsync did not save all points");

            }
            catch (Exception e)
            {

                Assert.Fail($"Unexpected exception of type {e.GetType()} caught: {e.Message}");
                return;
            }
        }

        [TestMethod, TestCategory("Post")]
        public async Task TestPostMixedPointAsync()
        {
            try
            {
                var client = new InfluxDBClient(influxUrl, dbUName, dbpwd);
                var time = DateTime.Now;
                var rand = new Random();
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
                var r = await client.PostPointAsync(dbName, valMixed);
                Assert.IsTrue(r, "PostPointAsync retunred false");
            }
            catch (Exception e)
            {

                Assert.Fail($"Unexpected exception of type {e.GetType()} caught: {e.Message}");
                return;
            }
        }

        [TestMethod, TestCategory("Post")]
        public async Task TestPostPointAsyncNonDefaultRetention()
        {
            try
            {
                var client = new InfluxDBClient(influxUrl, dbUName, dbpwd);
                var time = DateTime.Now;
                var rand = new Random();
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
                valMixed.Retention = new InfluxRetentionPolicy() { Duration = TimeSpan.FromHours(6) };

                var r = await client.PostPointAsync(dbName, valMixed);
                Assert.IsTrue(r && valMixed.Saved, "PostPointAsync retunred false");
            }
            catch (Exception e)
            {

                Assert.Fail($"Unexpected exception of type {e.GetType()} caught: {e.Message}");
                return;
            }
        }

        [TestMethod, TestCategory("Post")]
        public async Task TestPostPointsAsync_DifferentTypeFailure()
        {
            var client = new InfluxDBClient(influxUrl, dbUName, dbpwd);

            var points = new List<IInfluxDatapoint>();

            var firstPoint = new InfluxDatapoint<int>();
            firstPoint.UtcTimestamp = DateTime.UtcNow;
            firstPoint.Fields.Add("value", 1);
            firstPoint.MeasurementName = "SameKeyDifferentType";
            firstPoint.Precision = TimePrecision.Milliseconds;
            points.Add(firstPoint);


            var secondPoint = new InfluxDatapoint<double>();
            secondPoint.UtcTimestamp = DateTime.UtcNow;
            secondPoint.Fields.Add("value", 123.1234);
            secondPoint.MeasurementName = "SameKeyDifferentType";
            secondPoint.Precision = TimePrecision.Milliseconds;
            points.Add(secondPoint);


            var r = await AssertEx.ThrowsAsync<InfluxDBException>(() => client.PostPointsAsync(dbName, points));
        }

        [TestMethod, TestCategory("Get")]
        public async Task TestGetServerVersionAsync()
        {

            var client = new InfluxDBClient(influxUrl, dbUName, dbpwd);
            var version = await client.GetServerVersionAsync();

            Assert.IsFalse(String.IsNullOrWhiteSpace(version));
            Assert.IsTrue(version != "Unknown");
        }

        [TestMethod, TestCategory("Get")]
        public async Task TestGetRetentionPoliciesAsync()
        {
            var client = new InfluxDBClient(influxUrl, dbUName, dbpwd);
            var policies = await client.GetRetentionPoliciesAsync(dbName);
            Assert.IsNotNull(policies, "GetRetentionPoliciesAsync returned Null");
        }

        [TestMethod, TestCategory("Create")]
        public async Task TestCreateRetentionPolicy()
        {
            var client = new InfluxDBClient(influxUrl, dbUName, dbpwd);
            var p = new InfluxRetentionPolicy() { Name = "Test2", DBName = dbName, Duration = TimeSpan.FromMinutes(1150), IsDefault = false };

            var r = await client.CreateRetentionPolicyAsync(p);
            Assert.IsTrue(p.Saved, "CreateRetentionPolicyAsync failed");
        }

        [TestMethod, TestCategory("Get")]
        public async Task TestGetContinuousQueriesAsync()
        {
            var client = new InfluxDBClient(influxUrl, dbUName, dbpwd);
            var cqList = await client.GetContinuousQueriesAsync();

            Assert.IsNotNull(cqList, "GetContinuousQueriesAsync returned Null");

        }

        [TestMethod, TestCategory("Create")]
        public async Task TestCreateContinuousQueryAsync()
        {
            var client = new InfluxDBClient(influxUrl, dbUName, dbpwd);
            var query = $"select mean(Doublefield) as Doublefield into cqMeasurement from {measurementName} group by time(1h),*";
            var p = new InfluxContinuousQuery() { Name = "TestCQ", DBName = dbName, Query = query };

            var r = await client.CreateContinuousQueryAsync(p);
            Assert.IsTrue(r && p.Saved, "CreateContinuousQueryAsync failed");
        }


        [TestMethod, TestCategory("Create")]
        public async Task TestCreateContinuousQueryAsync_WithResample()
        {
            var client = new InfluxDBClient(influxUrl, dbUName, dbpwd);
            var query = $"select mean(Intfield) as Intfield into cqMeasurement from {measurementName} group by time(1h),*";
            var p = new InfluxContinuousQuery() { Name = "TestCQ1", DBName = dbName, Query = query, ResampleDuration = TimeSpan.FromHours(2), ResampleFrequency = TimeSpan.FromHours(0.5) };
            var r = await client.CreateContinuousQueryAsync(p);
            Assert.IsTrue(p.Saved, "CreateContinuousQueryAsync failed");
        }

        [TestMethod, TestCategory("Create")]
        public async Task TestCreateContinuousQueryAsync_MissingGroupBy()
        {
            var client = new InfluxDBClient(influxUrl, dbUName, dbpwd);
            var query = $"select mean(Doublefield) as Doublefield into cqMeasurement from {measurementName} group by *";
            var p = new InfluxContinuousQuery() { Name = "TestCQ2", DBName = dbName, Query = query };

            var r = await AssertEx.ThrowsAsync<InfluxDBException>(() => client.CreateContinuousQueryAsync(p));
        }

        [TestMethod, TestCategory("Drop")]
        public async Task TestDropContinuousQueryAsync()
        {
            var client = new InfluxDBClient(influxUrl, dbUName, dbpwd);
            var p = (await client.GetContinuousQueriesAsync()).FirstOrDefault();
            if (p != null)
            {
                var r = await client.DropContinuousQueryAsync(p);
                Assert.IsTrue(r && p.Deleted, "DropContinuousQueryAsync failed");
            }
        }

        [TestMethod, TestCategory("Drop")]
        public async Task TestDropContinuousQueryAsync_NotSaved()
        {
            var client = new InfluxDBClient(influxUrl, dbUName, dbpwd);
            var p = new InfluxContinuousQuery() { Name = "TestCQ2", DBName = dbName };

            var r = await AssertEx.ThrowsAsync<ArgumentException>(() => client.DropContinuousQueryAsync(p));
        }

        [TestMethod, TestCategory("Query")]
        public async Task TestQueryMultiSeriesAsync_Timeseries()
        {
            var client = new InfluxDBClient(influxUrl, dbUName, dbpwd);

            Stopwatch s = new Stopwatch();
            s.Start();
            var r = await client.QueryMultiSeriesAsync(internalDb, "show stats");

            s.Stop();
            Debug.WriteLine($"Elapsed{s.ElapsedMilliseconds}");
            Assert.IsTrue(r != null && r.Count > 0, "QueryMultiSeriesAsync retunred null or invalid data");
        }


        [TestMethod, TestCategory("Post")]
        public async Task TestPostPointsAsync_DifferentPrecisions()
        {
            try
            {
                var client = new InfluxDBClient(influxUrl, dbUName, dbpwd);
                var time = DateTime.Now;
                var today = DateTime.Now.ToShortDateString();
                var now = DateTime.Now.ToShortTimeString();

                var points = new List<IInfluxDatapoint>();

                foreach (TimePrecision precision in Enum.GetValues(typeof(TimePrecision)))
                {
                    var point = new InfluxDatapoint<long>();
                    point.UtcTimestamp = DateTime.UtcNow;
                    point.MeasurementName = $"Precision{precision.ToString()}";
                    point.Precision = precision;
                    point.Tags.Add("Precision", precision.ToString());
                    point.Fields.Add("Ticks", point.UtcTimestamp.Ticks);
                    points.Add(point);
                }

                var r = await client.PostPointsAsync(dbName, points);
                Assert.IsTrue(r, "PostPointsAsync retunred false");

                var values = await client.QueryMultiSeriesAsync(dbName, "select * from /Precision[A-Za-z]s/");
                foreach (var val in values)
                {
                    var x = val?.Entries?.FirstOrDefault();
                    try
                    {
                        var d = new DateTime(long.Parse(x.Ticks));
                        TimeSpan t = d - x.Time;
                        TimePrecision p = Enum.Parse(typeof(TimePrecision), x.Precision);
                        switch (p)
                        {
                            case TimePrecision.Hours: Assert.IsTrue(t.TotalHours < 1); break;
                            case TimePrecision.Minutes: Assert.IsTrue(t.TotalMinutes < 1); break;
                            case TimePrecision.Seconds: Assert.IsTrue(t.TotalSeconds < 1); break;
                            case TimePrecision.Milliseconds: Assert.IsTrue(t.TotalMilliseconds < 1); break;
                            case TimePrecision.Microseconds: Assert.IsTrue(t.Ticks < (TimeSpan.TicksPerMillisecond / 1000)); break;
                            case TimePrecision.Nanoseconds: Assert.IsTrue(t.Ticks < 1); break;
                        }
                    }
                    catch (Exception e)
                    {
                    }
                }
            }
            catch (Exception e)
            {
                Assert.Fail($"Unexpected exception of type {e.GetType()} caught: {e.Message}");
                return;
            }
        }

        [TestMethod, TestCategory("Query")]
        public async Task TestQueryMultiSeriesAsync_Chunking_BySeries()
        {
            try
            {
                var client = new InfluxDBClient(influxUrl, dbUName, dbpwd);
                var time = DateTime.Now;
                var TestDate = time.ToShortDateString();
                var TestTime = time.ToShortTimeString();
                var chunkSize = 10;
                var points = new List<IInfluxDatapoint>();

                for (var i = 0; i < chunkSize * 10; i++)
                {
                    await Task.Delay(1);
                    var point = new InfluxDatapoint<long>();
                    point.UtcTimestamp = DateTime.UtcNow;
                    point.MeasurementName = "ChunkTest";
                    point.Precision = TimePrecision.Nanoseconds;
                    point.Tags.Add("ChunkSeries", point.UtcTimestamp.Ticks % 2 == 0 ? "Chunk0" : "Chunk1");
                    point.Tags.Add("TestDate", TestDate);
                    point.Tags.Add("TestTime", TestTime);
                    point.Fields.Add("Val", i);
                    points.Add(point);
                }

                var r = await client.PostPointsAsync(dbName, points);
                Assert.IsTrue(r, "PostPointsAsync retunred false");

                var values = await client.QueryMultiSeriesAsync(dbName, $"select sum(Val) from ChunkTest where TestTime ='{ TestTime}' group by ChunkSeries", chunkSize * 10);
                foreach (var val in values)
                {
                    var x = val?.Entries?.Count;
                    //the series should be smaller than the chunk size, each resultset will only one series
                    Assert.IsTrue(x == 1);
                }
            }
            catch (Exception e)
            {
                Assert.Fail($"Unexpected exception of type {e.GetType()} caught: {e.Message}");
                return;
            }
        }

        [TestMethod, TestCategory("Query")]
        public async Task TestQueryMultiSeriesAsync_Chunking_BySize()
        {
            try
            {
                var client = new InfluxDBClient(influxUrl, dbUName, dbpwd);
                var time = DateTime.Now;
                var TestDate = time.ToShortDateString();
                var TestTime = time.ToShortTimeString();
                var chunkSize = 10;
                var points = new List<IInfluxDatapoint>();

                for (var i = 0; i < chunkSize * 10; i++)
                {
                    await Task.Delay(1);
                    var point = new InfluxDatapoint<long>();
                    point.UtcTimestamp = DateTime.UtcNow;
                    point.MeasurementName = "ChunkTest";
                    point.Precision = TimePrecision.Nanoseconds;
                    point.Tags.Add("ChunkSeries", point.UtcTimestamp.Ticks % 2 == 0 ? "Chunk0" : "Chunk1");
                    point.Tags.Add("TestDate", TestDate);
                    point.Tags.Add("TestTime", TestTime);
                    point.Fields.Add("Val", i);
                    points.Add(point);
                }

                var r = await client.PostPointsAsync(dbName, points);
                Assert.IsTrue(r, "PostPointsAsync retunred false");

                var values = await client.QueryMultiSeriesAsync(dbName, $"select Val from ChunkTest where TestTime ='{ TestTime}' limit {chunkSize * 5}", chunkSize);
                foreach (var val in values)
                {
                    var x = val?.Entries?.Count;
                    Assert.IsTrue(x == chunkSize);
                }
            }
            catch (Exception e)
            {
                Assert.Fail($"Unexpected exception of type {e.GetType()} caught: {e.Message}");
                return;
            }
        }

        [TestMethod, TestCategory("Post")]
        public async Task TestPostPointsAsync_OlderthanRetention()
        {
            try
            {
                var client = new InfluxDBClient(influxUrl, dbUName, dbpwd);
                var time = DateTime.Now;
                var TestDate = time.ToShortDateString();
                var TestTime = time.ToShortTimeString();
                var points = new List<IInfluxDatapoint>();
                InfluxRetentionPolicy retention = new InfluxRetentionPolicy() { Duration = TimeSpan.FromHours(1) };

                for (var i = 0; i < 10; i++)
                {
                    await Task.Delay(1);
                    var point = new InfluxDatapoint<long>();
                    point.Retention = retention;
                    point.UtcTimestamp = DateTime.UtcNow.AddDays(-i);
                    point.MeasurementName = "RetentionTest";
                    point.Precision = TimePrecision.Nanoseconds;
                    point.Tags.Add("TestDate", TestDate);
                    point.Tags.Add("TestTime", TestTime);
                    point.Fields.Add("Val", i);
                    points.Add(point);
                }

                var r = await client.PostPointsAsync(dbName, points);
                Assert.IsTrue(r, "PostPointsAsync retunred false");

                Assert.IsTrue(points.Count(p => p.Saved) == 1, "PostPointsAsync saved points older than retention policy");

            }
            catch (Exception e)
            {
                Assert.Fail($"Unexpected exception of type {e.GetType()} caught: {e.Message}");
                return;
            }
        }

        [TestMethod, TestCategory("Post")]
        public async Task TestPostPointsAsync_DefaultTimePrecision()
        {
            try
            {
                var client = new InfluxDBClient(influxUrl, dbUName, dbpwd);
                var time = DateTime.Now;
                var TestDate = time.ToShortDateString();
                var TestTime = time.ToShortTimeString();
                var points = new List<IInfluxDatapoint>();
                for (var i = 0; i < 10; i++)
                {
                    await Task.Delay(1);
                    var point = new InfluxDatapoint<long>();
                    point.MeasurementName = "DefaultPrecisionTest";
                    point.Tags.Add("TestDate", TestDate);
                    point.Tags.Add("TestTime", TestTime);
                    point.Fields.Add("Val", i);
                    points.Add(point);
                }

                var r = await client.PostPointsAsync(dbName, points);
                Assert.IsTrue(r, "PostPointsAsync retunred false");
            }
            catch (Exception e)
            {
                Assert.Fail($"Unexpected exception of type {e.GetType()} caught: {e.Message}");
                return;
            }
        }

    }
}