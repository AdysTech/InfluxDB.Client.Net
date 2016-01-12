using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Dynamic;
using System.Linq;
using AdysTech.InfluxDB.Client.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;

namespace InfluxDB.Client.Test
{
    [TestClass]
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
        [ExpectedException (typeof (ServiceUnavailableException))]
        public async Task TestGetInfluxDBNamesAsync_ServiceUnavailable()
        {
            var client = new InfluxDBClient (invalidInfluxUrl);
            var r = await client.GetInfluxDBNamesAsync ();
        }

        [TestMethod]
        [ExpectedException (typeof (UnauthorizedAccessException))]
        public async Task TestGetInfluxDBNamesAsync_Auth()
        {
            var client = new InfluxDBClient (influxUrl);
            var r = await client.GetInfluxDBNamesAsync ();
        }


        [TestMethod]
        [ExpectedException (typeof (UnauthorizedAccessException))]
        public async Task TestGetInfluxDBNamesAsync_Auth2()
        {
            var client = new InfluxDBClient (influxUrl, invalidUName, dbpwd);
            var r = await client.GetInfluxDBNamesAsync ();
        }



        [TestMethod]
        public async Task TestGetInfluxDBNamesAsync()
        {
            try
            {

                var client = new InfluxDBClient (influxUrl, dbUName, dbpwd);
                var r = await client.GetInfluxDBNamesAsync ();
                Assert.IsTrue (r != null && r.Count > 0, "GetInfluxDBNamesAsync retunred null or empty collection");
            }
            catch ( Exception e )
            {
                Assert.Fail ("Unexpected exception of type {0} caught: {1}",
                            e.GetType (), e.Message);
                return;
            }
        }

        [TestMethod]
        public async Task TestGetInfluxDBStructureAsync()
        {
            try
            {

                var client = new InfluxDBClient (influxUrl, dbUName, dbpwd);
                var r = await client.GetInfluxDBStructureAsync ("InvalidDB");
                Assert.IsTrue (r != null && r.Count == 0, "GetInfluxDBNamesAsync retunred null or non empty collection");
            }
            catch ( Exception e )
            {
                Assert.Fail ("Unexpected exception of type {0} caught: {1}",
                            e.GetType (), e.Message);
                return;
            }
        }

        [TestMethod]
        public async Task TestGetInfluxDBStructureAsync_InvalidDB()
        {
            try
            {

                var client = new InfluxDBClient (influxUrl, dbUName, dbpwd);
                var r = await client.GetInfluxDBStructureAsync (dbName);
                Assert.IsTrue (r != null && r.Count >= 0, "GetInfluxDBNamesAsync retunred null or non empty collection");
            }
            catch ( Exception e )
            {
                Assert.Fail ("Unexpected exception of type {0} caught: {1}",
                            e.GetType (), e.Message);
                return;
            }
        }

        [TestMethod]
        [ExpectedException (typeof (ArgumentException))]
        public async Task TestCreateDatabaseAsync_InvalidName()
        {
            var client = new InfluxDBClient (influxUrl, dbUName, dbpwd);
            var r = await client.CreateDatabaseAsync (invalidDbName);
        }

        [TestMethod]
        public async Task TestCreateDatabaseAsync()
        {
            try
            {

                var client = new InfluxDBClient (influxUrl, dbUName, dbpwd);
                var r = await client.CreateDatabaseAsync (dbName);
                Assert.IsTrue (r, "CreateDatabaseAsync retunred false");
            }
            catch ( InvalidOperationException e )
            {
                StringAssert.Equals (e.Message, "database already exists");
            }
            catch ( Exception e )
            {

                Assert.Fail ("Unexpected exception of type {0} caught: {1}",
                            e.GetType (), e.Message);
                return;
            }
        }


        [TestMethod]
        public async Task TestQueryAsync()
        {
            try
            {

                var client = new InfluxDBClient (influxUrl, dbUName, dbpwd);
                var r = await client.QueryDBAsync ("stress", "select * from performance limit 10");
                DateTime d;
                Assert.IsTrue (r != null && DateTime.TryParse (r[0].time, out d), "QueryDBAsync retunred null or invalid data");
            }
            catch ( Exception e )
            {
                Assert.Fail ("Unexpected exception of type {0} caught: {1}",
                            e.GetType (), e.Message);
                return;
            }
        }

        [TestMethod]
        [ExpectedException (typeof (ArgumentException))]
        public async Task TestQueryAsync_MultiSeries()
        {
            var client = new InfluxDBClient (influxUrl, dbUName, dbpwd);
            var r = await client.QueryDBAsync ("_internal", "Show Series");
        }

        [TestMethod]
        public async Task TestPostIntPointAsync()
        {
            try
            {
                var client = new InfluxDBClient (influxUrl, dbUName, dbpwd);
                var time = DateTime.Now;
                var rand = new Random ();
                var valInt = new InfluxDatapoint<int> ();
                valInt.UtcTimestamp = DateTime.UtcNow;
                valInt.Tags.Add ("TestDate", time.ToShortDateString ());
                valInt.Tags.Add ("TestTime", time.ToShortTimeString ());
                valInt.Fields.Add ("Intfield", rand.Next ());
                valInt.Fields.Add ("Intfield2", rand.Next ());
                valInt.MeasurementName = measurementName;
                valInt.Precision = TimePrecision.Seconds;
                var r = await client.PostPointAsync (dbName, valInt);
                Assert.IsTrue (r, "PostPointAsync retunred false");
            }
            catch ( Exception e )
            {

                Assert.Fail ("Unexpected exception of type {0} caught: {1}",
                            e.GetType (), e.Message);
                return;
            }
        }

        [TestMethod]
        public async Task TestPostDoublePointAsync()
        {
            try
            {
                var client = new InfluxDBClient (influxUrl, dbUName, dbpwd);
                var time = DateTime.Now;
                var rand = new Random ();
                var valDouble = new InfluxDatapoint<double> ();
                valDouble.UtcTimestamp = DateTime.UtcNow;
                valDouble.Tags.Add ("TestDate", time.ToShortDateString ());
                valDouble.Tags.Add ("TestTime", time.ToShortTimeString ());
                valDouble.Fields.Add ("Doublefield", rand.NextDouble ());
                valDouble.Fields.Add ("Doublefield2", rand.NextDouble ());
                valDouble.MeasurementName = measurementName;
                valDouble.Precision = TimePrecision.Seconds;
                var r = await client.PostPointAsync (dbName, valDouble);
                Assert.IsTrue (r, "PostPointAsync retunred false");
            }
            catch ( Exception e )
            {

                Assert.Fail ("Unexpected exception of type {0} caught: {1}",
                            e.GetType (), e.Message);
                return;
            }
        }



        [TestMethod]
        public async Task TestPostStringPointAsync()
        {
            try
            {
                var client = new InfluxDBClient (influxUrl, dbUName, dbpwd);
                var time = DateTime.Now;
                var valString = new InfluxDatapoint<string> ();
                valString.UtcTimestamp = DateTime.UtcNow;
                valString.Tags.Add ("TestDate", time.ToShortDateString ());
                valString.Tags.Add ("TestTime", time.ToShortTimeString ());
                valString.Fields.Add ("Stringfield", @"j0,Oox7ju6lJvA0\");//DataGen.RandomString ());
                valString.MeasurementName = measurementName;
                valString.Precision = TimePrecision.Seconds;
                var r = await client.PostPointAsync (dbName, valString);
                Assert.IsTrue (r, "PostPointAsync retunred false");
            }
            catch ( Exception e )
            {

                Assert.Fail ("Unexpected exception of type {0} caught: {1}",
                            e.GetType (), e.Message);
                return;
            }
        }

        [TestMethod]
        public async Task TestPostBooleanPointAsync()
        {
            try
            {
                var client = new InfluxDBClient (influxUrl, dbUName, dbpwd);
                var time = DateTime.Now;

                var valBool = new InfluxDatapoint<bool> ();
                valBool.UtcTimestamp = DateTime.UtcNow;
                valBool.Tags.Add ("TestDate", time.ToShortDateString ());
                valBool.Tags.Add ("TestTime", time.ToShortTimeString ());
                valBool.Fields.Add ("Booleanfield", time.Ticks % 2 == 0);
                valBool.MeasurementName = measurementName;
                valBool.Precision = TimePrecision.Seconds;
                var r = await client.PostPointAsync (dbName, valBool);
                Assert.IsTrue (r, "PostPointAsync retunred false");
            }
            catch ( Exception e )
            {

                Assert.Fail ("Unexpected exception of type {0} caught: {1}",
                            e.GetType (), e.Message);
                return;
            }
        }

        [TestMethod]
        public async Task TestPostPointsAsync()
        {
            try
            {
                var client = new InfluxDBClient (influxUrl, dbUName, dbpwd);
                var time = DateTime.Now;
                var today = DateTime.Now.ToShortDateString ();
                var now = DateTime.Now.ToShortTimeString ();

                var points = new List<IInfluxDatapoint> ();

                var valDouble = new InfluxDatapoint<double> ();
                valDouble.UtcTimestamp = DateTime.UtcNow;
                valDouble.Tags.Add ("TestDate", today);
                valDouble.Tags.Add ("TestTime", now);
                valDouble.Fields.Add ("Doublefield", DataGen.RandomDouble ());
                valDouble.Fields.Add ("Doublefield2", DataGen.RandomDouble ());
                valDouble.MeasurementName = measurementName;
                valDouble.Precision = TimePrecision.Nanoseconds;
                points.Add (valDouble);

                valDouble = new InfluxDatapoint<double> ();
                valDouble.UtcTimestamp = DateTime.UtcNow;
                valDouble.Tags.Add ("TestDate", today);
                valDouble.Tags.Add ("TestTime", now);
                valDouble.Fields.Add ("Doublefield", DataGen.RandomDouble ());
                valDouble.Fields.Add ("Doublefield2", DataGen.RandomDouble ());
                valDouble.MeasurementName = measurementName;
                valDouble.Precision = TimePrecision.Microseconds;
                points.Add (valDouble);

                var valInt = new InfluxDatapoint<int> ();
                valInt.UtcTimestamp = DateTime.UtcNow;
                valInt.Tags.Add ("TestDate", today);
                valInt.Tags.Add ("TestTime", now);
                valInt.Fields.Add ("Intfield", DataGen.RandomInt ());
                valInt.Fields.Add ("Intfield2", DataGen.RandomInt ());
                valInt.MeasurementName = measurementName;
                valInt.Precision = TimePrecision.Milliseconds;
                points.Add (valInt);

                valInt = new InfluxDatapoint<int> ();
                valInt.UtcTimestamp = DateTime.UtcNow;
                valInt.Tags.Add ("TestDate", today);
                valInt.Tags.Add ("TestTime", now);
                valInt.Fields.Add ("Intfield", DataGen.RandomInt ());
                valInt.Fields.Add ("Intfield2", DataGen.RandomInt ());
                valInt.MeasurementName = measurementName;
                valInt.Precision = TimePrecision.Seconds;
                points.Add (valInt);

                var valBool = new InfluxDatapoint<bool> ();
                valBool.UtcTimestamp = DateTime.UtcNow;
                valBool.Tags.Add ("TestDate", today);
                valBool.Tags.Add ("TestTime", now);
                valBool.Fields.Add ("Booleanfield", time.Ticks % 2 == 0);
                valBool.MeasurementName = measurementName;
                valBool.Precision = TimePrecision.Minutes;
                points.Add (valBool);

                var valString = new InfluxDatapoint<string> ();
                valString.UtcTimestamp = DateTime.UtcNow;
                valString.Tags.Add ("TestDate", today);
                valString.Tags.Add ("TestTime", now);
                valString.Fields.Add ("Stringfield", DataGen.RandomString ());
                valString.MeasurementName = measurementName;
                valString.Precision = TimePrecision.Hours;
                points.Add (valString);


                var r = await client.PostPointsAsync (dbName, points);
                Assert.IsTrue (r, "PostPointsAsync retunred false");
            }
            catch ( Exception e )
            {

                Assert.Fail ("Unexpected exception of type {0} caught: {1}",
                            e.GetType (), e.Message);
                return;
            }
        }


        [TestMethod]
        [ExpectedException (typeof (InfluxDBException))]
        public async Task TestPostPointsAsync_PartialWrite()
        {
            var client = new InfluxDBClient (influxUrl, dbUName, dbpwd);
            var time = DateTime.Now;
            var today = DateTime.Now.ToShortDateString ();
            var now = DateTime.Now.ToShortTimeString ();

            var points = new List<IInfluxDatapoint> ();


            var valDouble = new InfluxDatapoint<double> ();
            valDouble.UtcTimestamp = DateTime.UtcNow;
            valDouble.Tags.Add ("TestDate", today);
            valDouble.Tags.Add ("TestTime", now);
            valDouble.Fields.Add ("Doublefield", DataGen.RandomDouble ());
            valDouble.Fields.Add ("Doublefield2", Double.NaN);
            valDouble.MeasurementName = measurementName;
            valDouble.Precision = TimePrecision.Seconds;
            points.Add (valDouble);


            for ( int i = 0; i < 5; i++ )
            {
                var valInt = new InfluxDatapoint<int> ();
                valInt.UtcTimestamp = DateTime.UtcNow.AddSeconds (-1 * DataGen.RandomInt (3600));
                valInt.Tags.Add ("TestDate", today);
                valInt.Tags.Add ("TestTime", now);
                valInt.Fields.Add ("Intfield", DataGen.RandomInt ());
                valInt.Fields.Add ("Intfield2", DataGen.RandomInt ());
                valInt.MeasurementName = measurementName;
                valInt.Precision = TimePrecision.Seconds;
                points.Add (valInt);
            }

            valDouble = new InfluxDatapoint<double> ();
            valDouble.UtcTimestamp = DateTime.UtcNow;
            valDouble.Tags.Add ("TestDate", today);
            valDouble.Tags.Add ("TestTime", now);
            valDouble.Fields.Add ("Doublefield", DataGen.RandomDouble ());
            valDouble.Fields.Add ("Doublefield2", Double.NaN);
            valDouble.MeasurementName = measurementName;
            valDouble.Precision = TimePrecision.Seconds;
            points.Add (valDouble);

            var r = await client.PostPointsAsync (dbName, points);
            Assert.IsTrue (r, "PostPointsAsync retunred false");

        }

        /// <summary>
        /// This will create 2000 objects, and posts them to Influx. Since the precision is random, many points get overwritten 
        /// (e.g. you can have only point per hour at hour precision.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task TestBachingPostPointsAsync()
        {
            try
            {
                var client = new InfluxDBClient (influxUrl, dbUName, dbpwd);

                var points = new List<IInfluxDatapoint> ();

                var today = DateTime.Now.ToShortDateString ();
                var now = DateTime.Now.ToShortTimeString ();
                var start = DateTime.Now.AddDays (-5);
                var end = DateTime.Now;

                for ( int i = 0; i < 5000; i++ )
                {
                    var valDouble = new InfluxDatapoint<double> ();
                    valDouble.UtcTimestamp = DataGen.RandomDate (start, end);
                    valDouble.Tags.Add ("TestDate", today);
                    valDouble.Tags.Add ("TestTime", now);
                    valDouble.Fields.Add ("Doublefield", DataGen.RandomDouble ());
                    valDouble.Fields.Add ("Doublefield2", DataGen.RandomDouble ());
                    valDouble.MeasurementName = measurementName;
                    valDouble.Precision = (TimePrecision) ( DataGen.RandomInt () % 6 ) + 1;

                    var valInt = new InfluxDatapoint<int> ();
                    valInt.UtcTimestamp = DataGen.RandomDate (start, end);
                    valInt.Tags.Add ("TestDate", today);
                    valInt.Tags.Add ("TestTime", now);
                    valInt.Fields.Add ("Intfield", DataGen.RandomInt ());
                    valInt.Fields.Add ("Intfield2", DataGen.RandomInt ());
                    valInt.MeasurementName = measurementName;
                    valInt.Precision = (TimePrecision) ( DataGen.RandomInt () % 6 ) + 1;

                    var valBool = new InfluxDatapoint<bool> ();
                    valBool.UtcTimestamp = DataGen.RandomDate (start, end);
                    valBool.Tags.Add ("TestDate", today);
                    valBool.Tags.Add ("TestTime", now);
                    valBool.Fields.Add ("Booleanfield", DateTime.Now.Ticks % 2 == 0);
                    valBool.MeasurementName = measurementName;
                    valBool.Precision = (TimePrecision) ( DataGen.RandomInt () % 6 ) + 1;

                    var valString = new InfluxDatapoint<string> ();
                    valString.UtcTimestamp = DataGen.RandomDate (start, end);
                    valString.Tags.Add ("TestDate", today);
                    valString.Tags.Add ("TestTime", now);
                    valString.Fields.Add ("Stringfield", DataGen.RandomString ());
                    valString.MeasurementName = measurementName;
                    valString.Precision = (TimePrecision) ( DataGen.RandomInt () % 6 ) + 1;


                    points.Add (valString);
                    points.Add (valInt);
                    points.Add (valDouble);
                    points.Add (valBool);
                }

                var r = await client.PostPointsAsync (dbName, points);
                Assert.IsTrue (points.TrueForAll (p => p.Saved == true), "PostPointsAsync did not save all points");

            }
            catch ( Exception e )
            {

                Assert.Fail ("Unexpected exception of type {0} caught: {1}",
                            e.GetType (), e.Message);
                return;
            }
        }
    }

}
