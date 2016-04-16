using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Dynamic;
using System.Linq;
using AdysTech.InfluxDB.Client.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace AdysTech.InfluxDB.Client.Net.Tests
{
    [TestClass()]
    public class InfluxDBClientTest
    {

        const string dbName = "testDB";
        const string measurementName = "TestMeasurement";
        const string invalidDbName = "test DB";
        const string dbUName = "admin";
        const string dbpwd = "admin";
        const string invalidUName = "invalid";
        const string influxUrl = "http://localhost:8086";
        const string invalidInfluxUrl = "http://localhost:8089";


        [TestMethod]
        [ExpectedException(typeof(ServiceUnavailableException))]
        public async Task TestGetInfluxDBNamesAsync_ServiceUnavailable()
        {
            var client = new InfluxDBClient(invalidInfluxUrl);
            var r = await client.GetInfluxDBNamesAsync();
        }

        [TestMethod]
        public async Task TestGetInfluxDBNamesAsync()
        {
            try
            {

                var client = new InfluxDBClient(influxUrl, dbUName, dbpwd);
                Stopwatch s = new Stopwatch();
                s.Start();
                var r = await client.GetInfluxDBNamesAsync();
                s.Stop();
                Debug.WriteLine( s.ElapsedMilliseconds);

                Assert.IsTrue(r != null && r.Count > 0, "GetInfluxDBNamesAsync retunred null or empty collection");
            }
            catch (Exception e)
            {
                Assert.Fail("Unexpected exception of type {0} caught: {1}",
                            e.GetType(), e.Message);
                return;
            }
        }

        [TestMethod]
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
                Assert.Fail("Unexpected exception of type {0} caught: {1}",
                            e.GetType(), e.Message);
                return;
            }
        }

        [TestMethod]
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
                Assert.Fail("Unexpected exception of type {0} caught: {1}",
                            e.GetType(), e.Message);
                return;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task TestCreateDatabaseAsync_InvalidName()
        {
            var client = new InfluxDBClient(influxUrl, dbUName, dbpwd);
            var r = await client.CreateDatabaseAsync(invalidDbName);
        }

        [TestMethod]
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
                StringAssert.Equals(e.Message, "database already exists");
            }
            catch (Exception e)
            {

                Assert.Fail("Unexpected exception of type {0} caught: {1}",
                            e.GetType(), e.Message);
                return;
            }
        }


        [TestMethod]
        public async Task TestQueryAsync()
        {
            try
            {

                var client = new InfluxDBClient(influxUrl, dbUName, dbpwd);
                var r = await client.QueryDBAsync(dbName, "show measurements");
                Assert.IsTrue(r != null && r.Count>0, "QueryDBAsync retunred null or invalid data");
            }
            catch (Exception e)
            {
                Assert.Fail("Unexpected exception of type {0} caught: {1}",
                            e.GetType(), e.Message);
                return;
            }
        }

        [TestMethod]
        public async Task TestQueryMultiSeriesAsync()
        {
            var client = new InfluxDBClient(influxUrl, dbUName, dbpwd);
            
            Stopwatch s = new Stopwatch();
            s.Start();
            var r = await client.QueryMultiSeriesAsync(dbName, "SHOW STATS");

            s.Stop();
            Debug.WriteLine("Elapsed{0}", s.ElapsedMilliseconds);
            Assert.IsTrue(r != null && r.Count > 0, "QueryMultiSeriesAsync retunred null or invalid data");
        }



        [TestMethod]
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

                Assert.Fail("Unexpected exception of type {0} caught: {1}",
                            e.GetType(), e.Message);
                return;
            }
        }


        [TestMethod]
        [ExpectedException(typeof(InfluxDBException))]
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

            var r = await client.PostPointAsync(dbName, valDouble);
            Assert.IsTrue(r, "PostPointsAsync retunred false");

        }


        [TestMethod]
        [ExpectedException(typeof(InfluxDBException))]
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

            var r = await client.PostPointsAsync(dbName, points);
            Assert.IsTrue(r, "PostPointsAsync retunred false");

        }

        /// <summary>
        /// This will create 2000 objects, and posts them to Influx. Since the precision is random, many points get overwritten 
        /// (e.g. you can have only point per hour at hour precision.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
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

                Assert.Fail("Unexpected exception of type {0} caught: {1}",
                            e.GetType(), e.Message);
                return;
            }
        }

        [TestMethod]
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

                Assert.Fail("Unexpected exception of type {0} caught: {1}",
                            e.GetType(), e.Message);
                return;
            }
        }

        [TestMethod]
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
                valMixed.Retention = new InfluxRetentionPolicy() { Name = "Test2" };

                var r = await client.PostPointAsync(dbName, valMixed);
                Assert.IsTrue(r && valMixed.Saved, "PostPointAsync retunred false");
            }
            catch (Exception e)
            {

                Assert.Fail("Unexpected exception of type {0} caught: {1}",
                            e.GetType(), e.Message);
                return;
            }
        }

        [TestMethod()]
        public async Task TestGetServerVersionAsync()
        {

            var client = new InfluxDBClient(influxUrl, dbUName, dbpwd);
            var version = await client.GetServerVersionAsync();

            Assert.IsFalse(String.IsNullOrWhiteSpace(version));
            Assert.IsTrue(version != "Unknown");
        }

        [TestMethod()]
        public async Task TestGetRetentionPoliciesAsync()
        {
            var client = new InfluxDBClient(influxUrl, dbUName, dbpwd);
            var policies = await client.GetRetentionPoliciesAsync(dbName);
            Assert.IsNotNull(policies, "GetRetentionPoliciesAsync returned Null");
        }

        [TestMethod()]
        public async Task TestCreateRetentionPolicy()
        {
            var client = new InfluxDBClient(influxUrl, dbUName, dbpwd);
            var p = new InfluxRetentionPolicy() { Name = "Test2", DBName = dbName, Duration = TimeSpan.FromMinutes(1150), IsDefault = false };

            var r = await client.CreateRetentionPolicyAsync(p);
            Assert.IsTrue(p.Saved, "CreateRetentionPolicyAsync failed");
        }

     
    }

}
