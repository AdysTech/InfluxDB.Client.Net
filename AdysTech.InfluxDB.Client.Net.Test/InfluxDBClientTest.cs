using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using AdysTech.InfluxDB.Client.Net;
using System.Threading.Tasks;

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
        public async Task TestGetInfluxDBStructureAsync_InvalidDB()
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
        public async Task TestPostValueAsync()
        {
            try
            {
                var client = new InfluxDBClient (influxUrl, dbUName, dbpwd);
                var r = await client.PostValueAsync (dbName, measurementName, DateTime.Now.ToEpoch(TimePrecision.Seconds),TimePrecision.Seconds,"testTag=testTagValue","Temp",33.05);
                Assert.IsTrue (r, "PostValueAsync retunred false");
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
