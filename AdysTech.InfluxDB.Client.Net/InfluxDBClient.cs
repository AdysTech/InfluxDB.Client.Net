//Copyright: Adarsha@AdysTech

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AdysTech.InfluxDB.Client.Net.DataContracts;

namespace AdysTech.InfluxDB.Client.Net
{
    /// <summary>
    /// Represents precision of the point
    /// </summary>
    public enum TimePrecision
    {
        Hours = 1,
        Minutes = 2,
        Seconds = 3,
        Milliseconds = 4,
        Microseconds = 5,
        Nanoseconds = 6
    }


    /// <summary>
    /// Provides asynchronous interaction channel with InfluxDB engine
    /// </summary>
    public class InfluxDBClient : IInfluxDBClient
    {
        #region fields
        readonly string[] precisionLiterals = { "_", "h", "m", "s", "ms", "u", "n" };
        private readonly string _influxUrl;
        HttpClient _client;
        #endregion

        /// <summary>
        /// URL for InfluxDB Engine, include complete URL with http/s and port number
        /// </summary>
        public string InfluxUrl
        {
            get { return _influxUrl; }
        }

        private readonly string _influxDBUserName;
        /// <summary>
        /// User Name for InfluxDB if it requires authentication
        /// </summary>
        public string InfluxDBUserName
        {
            get { return _influxDBUserName; }
        }

        private readonly string _influxDBPassword;
        /// <summary>
        /// Password for InfluxDB if it requires authentication
        /// </summary>
        public string InfluxDBPassword
        {
            get { return _influxDBPassword; }
        }


        #region private methods

        private async Task<HttpResponseMessage> GetAsync(UriBuilder builder)
        {
            try
            {
                HttpResponseMessage response = await _client.GetAsync (builder.Uri);
                return response;
            }
            catch ( HttpRequestException e )
            {
                if ( e.InnerException is WebException && e.InnerException.Message == "Unable to connect to the remote server" )
                    throw new ServiceUnavailableException ();
            }
            return null;
        }

        private async Task<HttpResponseMessage> PostAsync(UriBuilder builder, ByteArrayContent requestContent)
        {

            try
            {
                HttpResponseMessage response = await _client.PostAsync (builder.Uri, requestContent);
                return response;
            }
            catch ( HttpRequestException e )
            {
                if ( e.InnerException is WebException && e.InnerException.Message == "Unable to connect to the remote server" )
                    throw new ServiceUnavailableException ();
            }
            return null;
        }

        private async Task<bool> PostPointsAsync(string dbName, TimePrecision precision, IEnumerable<IInfluxDatapoint> points)
        {
            var influxAddress = new Uri (String.Format ("{0}/write?", InfluxUrl));
            var builder = new UriBuilder (influxAddress);
            builder.Query = await new FormUrlEncodedContent (new[] { 
                    new KeyValuePair<string, string>("db", dbName) ,
                    new KeyValuePair<string, string>("precision", precisionLiterals[(int) precision])
                    }).ReadAsStringAsync ();

            var line = new StringBuilder ();
            foreach ( var point in points )
                line.AppendFormat ("{0}\n", point.ConvertToInfluxLineProtocol ());
            //remove last \n
            line.Remove (line.Length - 1, 1);

            ByteArrayContent requestContent = new ByteArrayContent (Encoding.UTF8.GetBytes (line.ToString ()));
            HttpResponseMessage response = await PostAsync (builder, requestContent);

            if ( response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.BadGateway || ( response.StatusCode == HttpStatusCode.InternalServerError && response.ReasonPhrase == "INKApi Error" ) ) //502 Connection refused
                throw new UnauthorizedAccessException ("InfluxDB needs authentication. Check uname, pwd parameters");
            //if(response.StatusCode==HttpStatusCode.NotFound)
            else if ( response.StatusCode == HttpStatusCode.NoContent )
                return true;
            else
                return false;
        }
        #endregion

        /// <summary>
        /// Creates the InfluxDB Client
        /// </summary>
        /// <param name="InfluxUrl">Url for the Inflex Server, e.g. http://localhost:8086</param>
        /// <param name="UserName">User name to authenticate InflexDB</param>
        /// <param name="Password">password</param>
        public InfluxDBClient(string InfluxUrl, string UserName, string Password)
        {
            this._influxUrl = InfluxUrl;
            this._influxDBUserName = UserName;
            this._influxDBPassword = Password;

            HttpClientHandler handler = new HttpClientHandler ();
            handler.UseDefaultCredentials = true;
            handler.PreAuthenticate = true;
            handler.Proxy = WebRequest.DefaultWebProxy;
            WebRequest.DefaultWebProxy.Credentials = CredentialCache.DefaultNetworkCredentials;

            _client = new HttpClient (handler);
            if ( !( String.IsNullOrWhiteSpace (InfluxDBUserName) && String.IsNullOrWhiteSpace (InfluxDBPassword) ) )
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue ("Basic",
                    Convert.ToBase64String (System.Text.ASCIIEncoding.ASCII.GetBytes (string.Format ("{0}:{1}", InfluxDBUserName, InfluxDBPassword))));
        }

        /// <summary>
        /// Creates the InfluxDB Client
        /// </summary>
        /// <param name="InfluxUrl">Url for the Inflex Server, e.g. localhost:8086</param>
        public InfluxDBClient(string InfluxUrl)
            : this (InfluxUrl, null, null)
        {

        }



        /// <summary>
        /// Queries and Gets list of all existing databases in the Influx server instance
        /// </summary>
        /// <returns>List of DB names, empty list incase of an error</returns>
        ///<exception cref="UnauthorizedAccessException">When Influx needs authentication, and no user name password is supplied or auth fails</exception>
        ///<exception cref="HttpRequestException">all other HTTP exceptions</exception>
        ///<exception cref="ServiceUnavailableException">InfluxDB service is not available on the port mentioned</exception>
        public async Task<List<String>> GetInfluxDBNamesAsync()
        {
            var dbNames = new List<String> ();
            var query = new Uri (InfluxUrl + "/query?");
            var builder = new UriBuilder (query);
            //builder.UserName = influxDBUserName;
            //builder.Password = influxDBPassword;
            builder.Query = await new FormUrlEncodedContent (new[] { 
                    //new KeyValuePair<string, string>("u",InfluxDBUserName) ,
                    //new KeyValuePair<string, string>("p", InfluxDBPassword) ,
                    new KeyValuePair<string, string>("q", "SHOW DATABASES") 
                    }).ReadAsStringAsync ();
            var response = await GetAsync (builder);
            if ( response.StatusCode == HttpStatusCode.OK )
            {
                var content = await response.Content.ReadAsStringAsync ();
                dbNames.AddRange (Regex.Matches (content, "([a-zA-Z0-9]+)").Cast<Match> ().Select (match => match.Value).SkipWhile (p => p != "values").Skip (1));
            }
            else if ( response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.BadGateway || ( response.StatusCode == HttpStatusCode.InternalServerError && response.ReasonPhrase == "INKApi Error" ) ) //502 Connection refused
                throw new UnauthorizedAccessException ("InfluxDB needs authentication. Check uname, pwd parameters");

            return dbNames;
        }


        /// <summary>
        /// Gets the whole DB structure for the given databse in Influx.
        /// </summary>
        /// <param name="dbName">Name of the database</param>
        /// <returns>Hierarchical structure, Dictionary<string:measurement, List<field names>></returns>
        ///<exception cref="UnauthorizedAccessException">When Influx needs authentication, and no user name password is supplied or auth fails</exception>
        ///<exception cref="HttpRequestException">all other HTTP exceptions</exception>
        public async Task<Dictionary<string, List<String>>> GetInfluxDBStructureAsync(string dbName)
        {
            var dbStructure = new Dictionary<string, List<string>> ();
            var query = new Uri (InfluxUrl + "/query?");
            var builder = new UriBuilder (query);
            builder.Query = await new FormUrlEncodedContent (new[] { 
					new KeyValuePair<string, string>("db", dbName) ,
					new KeyValuePair<string, string>("q", "SHOW FIELD KEYS") 
					}).ReadAsStringAsync ();
            var response = await GetAsync (builder);
            if ( response.StatusCode == HttpStatusCode.OK )
            {
                var content = await response.Content.ReadAsStringAsync ();
                var values = Regex.Matches (content, "([a-zA-Z0-9_]+)").Cast<Match> ().Select (match => match.Value).ToList ();
                string measurement;
                //one pass loop through the entries in returned structure. Each new measurement starts with "name",measurement name, "columns","fieldKey","values",list of columns
                //we will search for name, and once found grab measurement name, skip 3 lines, and grab column names
                for ( int i = 0; i < values.Count; i++ )
                {
                    if ( values[i] != "name" )
                        continue;
                    if ( values[i] == "name" )
                    {
                        if ( ++i == values.Count )
                            throw new InvalidDataException ("Invalid data returned from InfluxDB");
                        //i is incremented
                        measurement = values[i];
                        dbStructure.Add (measurement, new List<string> ());
                        for ( int j = i + 4; j < values.Count; j++ )
                        {
                            if ( values[j] == "name" )
                            {
                                i = j - 1;
                                break;
                            }
                            dbStructure[measurement].Add (values[j]);
                        }
                    }
                }
            }
            else if ( response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.BadGateway || ( response.StatusCode == HttpStatusCode.InternalServerError && response.ReasonPhrase == "INKApi Error" ) ) //502 Connection refused
                throw new UnauthorizedAccessException ("InfluxDB needs authentication. Check uname, pwd parameters");

            return dbStructure;
        }

        /// <summary>
        /// Creates the specified database
        /// </summary>
        /// <param name="dbName"></param>
        /// <returns>True:success, Fail:Failed to create db</returns>
        ///<exception cref="UnauthorizedAccessException">When Influx needs authentication, and no user name password is supplied or auth fails</exception>
        ///<exception cref="HttpRequestException">all other HTTP exceptions</exception>
        public async Task<bool> CreateDatabaseAsync(string dbName)
        {
            var query = new Uri (InfluxUrl + "/query?");
            var builder = new UriBuilder (query);
            builder.Query = await new FormUrlEncodedContent (new[] { 
                    new KeyValuePair<string, string>("q", "CREATE DATABASE "+ dbName) 
                    }).ReadAsStringAsync ();
            var response = await GetAsync (builder);
            if ( response.StatusCode == HttpStatusCode.OK )
            {
                var content = await response.Content.ReadAsStringAsync ();
                if ( content.Contains ("database already exists") )
                    throw new InvalidOperationException ("database already exists");
                return true;
            }
            else if ( response.StatusCode == HttpStatusCode.BadRequest )
                throw new ArgumentException ("Invalid DB Name");
            else if ( response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.BadGateway || ( response.StatusCode == HttpStatusCode.InternalServerError && response.ReasonPhrase == "INKApi Error" ) ) //502 Connection refused
                throw new UnauthorizedAccessException ("InfluxDB needs authentication. Check uname, pwd parameters");

            return false;
        }

        /// <summary>
        /// Posts raw write request to Influx.
        /// </summary>
        /// <param name="dbName">Name of the Database</param>
        /// <param name="precision">Unit of the timestamp, Hour->nanosecond</param>
        /// <param name="content">Raw request, as per Line Protocol</param>
        /// <see cref="https://influxdb.com/docs/v0.9/write_protocols/write_syntax.html#http"/>
        /// <returns>true:success, false:failure</returns>
        public async Task<bool> PostRawValueAsync(string dbName, TimePrecision precision, string content)
        {
            var influxAddress = new Uri (String.Format ("{0}/write?", InfluxUrl));
            var builder = new UriBuilder (influxAddress);
            builder.Query = await new FormUrlEncodedContent (new[] { 
                    new KeyValuePair<string, string>("db", dbName) ,
                    new KeyValuePair<string, string>("precision", precisionLiterals[(int) precision])
                    }).ReadAsStringAsync ();


            ByteArrayContent requestContent = new ByteArrayContent (Encoding.UTF8.GetBytes (content));
            HttpResponseMessage response = await PostAsync (builder, requestContent);

            if ( response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.BadGateway || ( response.StatusCode == HttpStatusCode.InternalServerError && response.ReasonPhrase == "INKApi Error" ) ) //502 Connection refused
                throw new UnauthorizedAccessException ("InfluxDB needs authentication. Check uname, pwd parameters");
            //if(response.StatusCode==HttpStatusCode.NotFound)

            else if ( response.StatusCode == HttpStatusCode.NoContent )
                return true;
            else
                return false;
        }

        /// <summary>
        /// Queries Influx DB and gets a time series data back. Ideal for fetching measurement values.
        /// The return list is of dynamics, and each element in there will have properties named after columns in series
        /// </summary>
        /// <param name="dbName">Name of the database</param>
        /// <param name="measurementQuery">Query text, Only results with single series are supported for now</param>
        /// <returns>List of ExpandoObjects (in the form of dynamic). 
        /// The objects will have columns as Peoperties with their current values</returns>
        public async Task<List<dynamic>> QueryDBAsync(string dbName, string measurementQuery)
        {
            var dbStructure = new Dictionary<string, List<string>> ();
            var query = new Uri (InfluxUrl + "/query?");
            var builder = new UriBuilder (query);
            builder.Query = await new FormUrlEncodedContent (new[] { 
					new KeyValuePair<string, string>("db", dbName) ,
					new KeyValuePair<string, string>("q", measurementQuery) 
					}).ReadAsStringAsync ();
            var response = await GetAsync (builder);
            if ( response.StatusCode == HttpStatusCode.OK )
            {
                var content = await response.Content.ReadAsStreamAsync ();
                DataContractJsonSerializer js = new DataContractJsonSerializer (typeof (InfluxResponse));
                var result = js.ReadObject (content) as InfluxResponse;

                if ( result.Results.Count > 1 )
                    throw new ArgumentException ("The query is resulting in Multi Series respone, which is not supported by this method");

                if ( result.Results[0].Series.Count > 1 )
                    throw new ArgumentException ("The query is resulting in Multi Series respone, which is not supported by this method");

                var series = result.Results[0].Series[0];

                var results = new List<dynamic> ();
                for ( var row = 0; row < series.Values.Count; row++ )
                {
                    dynamic entry = new ExpandoObject ();
                    results.Add (entry);
                    for ( var col = 0; col < series.ColumnHeaders.Count; col++ )
                    {
                        ( (IDictionary<string, object>) entry ).Add (series.ColumnHeaders[col], series.Values[row][col]);
                    }
                }
                return results;
            }
            return null;
        }

        /// <summary>
        /// Posts an InfluxDataPoint to given measurement
        /// </summary>
        /// <param name="dbName">InfluxDB database name</param>
        /// <param name="point">Influx data point to be written</param>
        /// <returns>True:Success, False:Failure</returns>
        ///<exception cref="UnauthorizedAccessException">When Influx needs authentication, and no user name password is supplied or auth fails</exception>
        ///<exception cref="HttpRequestException">all other HTTP exceptions</exception>   
        public async Task<bool> PostPointAsync(string dbName, IInfluxDatapoint point)
        {
            var influxAddress = new Uri (String.Format ("{0}/write?", InfluxUrl));
            var builder = new UriBuilder (influxAddress);
            builder.Query = await new FormUrlEncodedContent (new[] { 
                    new KeyValuePair<string, string>("db", dbName) ,
                    new KeyValuePair<string, string>("precision", precisionLiterals[(int) point.Precision])
                    }).ReadAsStringAsync ();

            ByteArrayContent requestContent = new ByteArrayContent (Encoding.UTF8.GetBytes (point.ConvertToInfluxLineProtocol ()));
            HttpResponseMessage response = await PostAsync (builder, requestContent);

            if ( response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.BadGateway || ( response.StatusCode == HttpStatusCode.InternalServerError && response.ReasonPhrase == "INKApi Error" ) ) //502 Connection refused
                throw new UnauthorizedAccessException ("InfluxDB needs authentication. Check uname, pwd parameters");
            //if(response.StatusCode==HttpStatusCode.NotFound)
            else if ( response.StatusCode == HttpStatusCode.NoContent )
            {
                point.Saved = true;
                return true;
            }
            else
            {
                point.Saved = false;
                return false;
            }
        }


        /// <summary>
        /// Posts series of InfluxDataPoints to given measurement, in batches of 255
        /// </summary>
        /// <param name="dbName">InfluxDB database name</param>
        /// <param name="Points">Collection of Influx data points to be written</param>
        /// <returns>True:Success, False:Failure</returns>
        ///<exception cref="UnauthorizedAccessException">When Influx needs authentication, and no user name password is supplied or auth fails</exception>
        ///<exception cref="HttpRequestException">all other HTTP exceptions</exception>   
        public async Task<bool> PostPointsAsync(string dbName, IEnumerable<IInfluxDatapoint> Points)
        {
            int maxBatchSize = 255;

            foreach ( var group in Points.GroupBy (p => p.Precision) )
            {
                var pointsGroup = group.AsEnumerable ().Select ((point, index) => new { Index = index, Point = point })//get the index of each point
                     .GroupBy (x => x.Index / maxBatchSize) //chunk into smaller batches
                     .Select (x => x.Select (v => v.Point)); //get the points
                foreach ( var points in pointsGroup )
                {
                    if ( await PostPointsAsync (dbName, group.Key, points) )
                    {
                        points.ToList ().ForEach (p => p.Saved = true);
                    }
                }

            }

            return true;
        }



        #region obselete methods

        /// <summary>
        /// Posts one set of values (i.e. multiple fields) to a given measurement
        /// </summary>
        /// <param name="dbName">Name of the Database</param>
        /// <param name="measurement">Name of the Measurement</param>
        /// <param name="timestamp">Timestamp for the value, EPOCH</param>
        /// <param name="precision">Unit of the timestamp, Hour->nanosecond</param>
        /// <param name="tags">Tags for the value</param>
        /// <param name="field">Filed Name</param>
        /// <param name="value">Value, double, will be formated with 0.00</param>
        /// <returns>True:Success, False:Failure</returns>
        ///<exception cref="UnauthorizedAccessException">When Influx needs authentication, and no user name password is supplied or auth fails</exception>
        ///<exception cref="HttpRequestException">all other HTTP exceptions</exception>   
        [Obsolete ("PostValueAsync is deprecated, please use PostDataPointAsync instead.")]
        public async Task<bool> PostValueAsync(string dbName, string measurement, long timestamp, TimePrecision precision, string tags, string field, double value)
        {
            var influxAddress = new Uri (String.Format ("{0}/write?", InfluxUrl));
            var builder = new UriBuilder (influxAddress);
            builder.Query = await new FormUrlEncodedContent (new[] { 
                    new KeyValuePair<string, string>("db", dbName) ,
                    new KeyValuePair<string, string>("precision", precisionLiterals[(int) precision])
                    }).ReadAsStringAsync ();

            var content = String.Format (System.Globalization.CultureInfo.GetCultureInfo ("en-US"), "{0},{1} {2}={3} {4}", measurement, tags, field, value, timestamp);
            ByteArrayContent requestContent = new ByteArrayContent (Encoding.UTF8.GetBytes (content));
            HttpResponseMessage response = await PostAsync (builder, requestContent);

            if ( response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.BadGateway || ( response.StatusCode == HttpStatusCode.InternalServerError && response.ReasonPhrase == "INKApi Error" ) ) //502 Connection refused
                throw new UnauthorizedAccessException ("InfluxDB needs authentication. Check uname, pwd parameters");
            //if(response.StatusCode==HttpStatusCode.NotFound)
            else if ( response.StatusCode == HttpStatusCode.NoContent )
                return true;
            else
                return false;
        }



        /// <summary>
        /// Posts one set of values (i.e. multiple fields) to a given measurement
        /// </summary>
        /// <param name="dbName">Name of the Database</param>
        /// <param name="measurement">Name of the Measurement</param>
        /// <param name="timestamp">Timestamp for the value, EPOCH</param>
        /// <param name="precision">Unit of the timestamp, Hour->nanosecond</param>
        /// <param name="tags">Tags for the value</param>
        /// <param name="values">Values, in Field=Value format</param>
        /// <returns>True:Success, False:Failure</returns>
        ///<exception cref="UnauthorizedAccessException">When Influx needs authentication, and no user name password is supplied or auth fails</exception>
        ///<exception cref="HttpRequestException">all other HTTP exceptions</exception>   
        [Obsolete ("PostValuesAsync is deprecated, please use PostDataPointsAsync instead.")]
        public async Task<bool> PostValuesAsync(string dbName, string measurement, long timestamp, TimePrecision precision, string tags, IDictionary<string, double> values)
        {
            var influxAddress = new Uri (String.Format ("{0}/write?", InfluxUrl));
            var builder = new UriBuilder (influxAddress);
            builder.Query = await new FormUrlEncodedContent (new[] { 
                    new KeyValuePair<string, string>("db", dbName) ,
                    new KeyValuePair<string, string>("precision", precisionLiterals[(int) precision])
                    }).ReadAsStringAsync ();

            //var content = new StringBuilder ();
            //foreach ( var value in values )
            //    content.AppendFormat ("{0},{1} {2} {3}\n", measurement, tags, value, timestamp);
            ////remove last \n
            //content.Remove (content.Length - 1, 1);
            var valuesTxt = String.Join (",", values.Select (v => String.Format (System.Globalization.CultureInfo.GetCultureInfo ("en-US"), "{0}={1}", v.Key, v.Value)));
            var content = String.Format ("{0},{1} {2} {3}", measurement, tags, valuesTxt, timestamp);

            ByteArrayContent requestContent = new ByteArrayContent (Encoding.UTF8.GetBytes (content.ToString ()));
            HttpResponseMessage response = await PostAsync (builder, requestContent);

            if ( response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.BadGateway || ( response.StatusCode == HttpStatusCode.InternalServerError && response.ReasonPhrase == "INKApi Error" ) ) //502 Connection refused
                throw new UnauthorizedAccessException ("InfluxDB needs authentication. Check uname, pwd parameters");
            //if(response.StatusCode==HttpStatusCode.NotFound)

            else if ( response.StatusCode == HttpStatusCode.NoContent )
                return true;
            else
                return false;
        }

        #endregion
    }

}
