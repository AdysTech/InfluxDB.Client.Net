//Copyright: Adarsha@AdysTech

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AdysTech.InfluxDB.Client.Net.DataContracts;
using System.Web.Script.Serialization;

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

        public string InfluxServer
        {
            get
            {
                if (_influxUrl != null)
                    return new Uri(_influxUrl).Host;
                return null;
            }
        }

        public int Port
        {
            get
            {
                if (_influxUrl != null)
                    return new Uri(_influxUrl).Port;
                return -1;
            }
        }


        #region private methods

        private async Task<HttpResponseMessage> GetAsync(Dictionary<string, string> Query)
        {
            var querybaseUrl = new Uri(String.Format("{0}/query?", InfluxUrl));
            var builder = new UriBuilder(querybaseUrl);

            if (InfluxDBUserName != null && !Query.ContainsKey("u"))
                Query.Add("u", InfluxDBUserName);
            if (InfluxDBPassword != null && !Query.ContainsKey("p"))
                Query.Add("p", InfluxDBPassword);
            builder.Query = await new FormUrlEncodedContent(Query).ReadAsStringAsync();

            try
            {
                HttpResponseMessage response = await _client.GetAsync(builder.Uri);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return response;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.BadGateway || (response.StatusCode == HttpStatusCode.InternalServerError && response.ReasonPhrase == "INKApi Error")) //502 Connection refused
                    throw new UnauthorizedAccessException("InfluxDB needs authentication. Check uname, pwd parameters");
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    throw new ArgumentException("Invalid Query resulted in an {0}", error.Substring(10));
                }

            }
            catch (HttpRequestException e)
            {
                if (e.InnerException is WebException && e.InnerException.Message == "Unable to connect to the remote server")
                    throw new ServiceUnavailableException();
            }
            return null;
        }


        private async Task<HttpResponseMessage> PostAsync(Dictionary<string, string> EndPoint, ByteArrayContent requestContent)
        {

            var querybaseUrl = new Uri(String.Format("{0}/write?", InfluxUrl));
            var builder = new UriBuilder(querybaseUrl);

            if (!EndPoint.ContainsKey("u"))
                EndPoint.Add("u", InfluxDBUserName);
            if (!EndPoint.ContainsKey("p"))
                EndPoint.Add("p", InfluxDBPassword);
            builder.Query = await new FormUrlEncodedContent(EndPoint).ReadAsStringAsync();


            try
            {
                HttpResponseMessage response = await _client.PostAsync(builder.Uri, requestContent);

                if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.BadGateway || (response.StatusCode == HttpStatusCode.InternalServerError && response.ReasonPhrase == "INKApi Error")) //502 Connection refused
                    throw new UnauthorizedAccessException("InfluxDB needs authentication. Check uname, pwd parameters");

                return response;
            }
            catch (HttpRequestException e)
            {
                if (e.InnerException is WebException && e.InnerException.Message == "Unable to connect to the remote server")
                    throw new ServiceUnavailableException();
            }
            return null;
        }

        private async Task<bool> PostPointsAsync(string dbName, TimePrecision precision, string retention, IEnumerable<IInfluxDatapoint> points)
        {
            Regex multiLinePattern = new Regex(@"([\P{Cc}].*?) '([\P{Cc}].*?)':([\P{Cc}].*?)\\n", RegexOptions.Compiled, TimeSpan.FromSeconds(5));
            Regex oneLinePattern = new Regex(@"{\""error"":""([9\P{Cc}]+) '([\P{Cc}]+)':([a-zA-Z0-9 ]+)", RegexOptions.Compiled, TimeSpan.FromSeconds(5));
            Regex errorLinePattern = new Regex(@"{\""error"":""([9\P{Cc}]+) '([\P{Cc}]+)':([a-zA-Z0-9 ]+)", RegexOptions.Compiled, TimeSpan.FromSeconds(5));

            var line = new StringBuilder();
            foreach (var point in points)
                line.AppendFormat("{0}\n", point.ConvertToInfluxLineProtocol());
            //remove last \n
            line.Remove(line.Length - 1, 1);

            ByteArrayContent requestContent = new ByteArrayContent(Encoding.UTF8.GetBytes(line.ToString()));
            HttpResponseMessage response = await PostAsync(new Dictionary<string, string>() {
                { "db", dbName },
                { "rp", retention ?? "default" },
                { "precision", precisionLiterals[(int)precision] } }, requestContent);

            if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.BadGateway || (response.StatusCode == HttpStatusCode.InternalServerError && response.ReasonPhrase == "INKApi Error")) //502 Connection refused
                throw new UnauthorizedAccessException("InfluxDB needs authentication. Check uname, pwd parameters");
            //if(response.StatusCode==HttpStatusCode.NotFound)
            else if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var content = await response.Content.ReadAsStringAsync();
                //regex assumes error text from https://github.com/influxdata/influxdb/blob/master/models/points.go ParsePointsWithPrecision
                //fmt.Sprintf("'%s': %v", string(block[start:len(block)])
                List<string> parts=null; bool partialWrite =false; string l="";
                try
                {
                    if (content.Contains("partial write"))
                    {
                        if (content.Contains("\\n"))
                            parts = multiLinePattern.Matches(content.Substring(content.IndexOf("partial write:\\n") + 16)).ToList();
                        else
                            parts = oneLinePattern.Matches(content.Substring(content.IndexOf("partial write:\\n") + 16)).ToList();
                        partialWrite = true;
                    }
                    else
                    {
                        parts = errorLinePattern.Matches(content).ToList();
                        partialWrite = false;
                    }
                   
                    if (parts[1].Contains("\\n"))
                        l = parts[1].Substring(0, parts[1].IndexOf("\\n")).Unescape();
                    else
                        l = parts[1].Unescape();
                }
                catch (Exception)
                {
                   
                }

                var point = points.Where(p => p.ConvertToInfluxLineProtocol() == l).FirstOrDefault();
                if (point != null)
                    throw new InfluxDBException(partialWrite ? "Partial Write" : "Failed to Write", String.Format("{0}: {1} due to {2}", partialWrite ? "Partial Write" : "Failed to Write", parts?[0], parts?[2]), point);
                else
                    throw new InfluxDBException(partialWrite ? "Partial Write" : "Failed to Write", String.Format("{0}: {1} due to {2}", partialWrite ? "Partial Write" : "Failed to Write", parts?[0], parts?[2]), l);
                return false;
            }
            else if (response.StatusCode == HttpStatusCode.NoContent)
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

            HttpClientHandler handler = new HttpClientHandler();
            handler.UseDefaultCredentials = true;
            handler.PreAuthenticate = true;
            handler.Proxy = WebRequest.DefaultWebProxy;
            WebRequest.DefaultWebProxy.Credentials = CredentialCache.DefaultNetworkCredentials;

            _client = new HttpClient(handler);
            if (!(String.IsNullOrWhiteSpace(InfluxDBUserName) && String.IsNullOrWhiteSpace(InfluxDBPassword)))
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", InfluxDBUserName, InfluxDBPassword))));
            _client.DefaultRequestHeaders.ConnectionClose = false;
        }

        /// <summary>
        /// Creates the InfluxDB Client
        /// </summary>
        /// <param name="InfluxUrl">Url for the Inflex Server, e.g. localhost:8086</param>
        public InfluxDBClient(string InfluxUrl)
            : this(InfluxUrl, null, null)
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
            var dbNames = new List<String>();

            var dbs = await QueryDBAsync(null, "SHOW DATABASES");

            foreach (var db in dbs)
                dbNames.Add(db?.Name);

            return dbNames;
        }


        /// <summary>
        /// Gets the whole DB structure for the given databse in Influx.
        /// </summary>
        /// <param name="dbName">Name of the database</param>
        /// <returns>Hierarchical structure, Database<Measurement<Tags,Fields>></returns>
        ///<exception cref="UnauthorizedAccessException">When Influx needs authentication, and no user name password is supplied or auth fails</exception>
        ///<exception cref="HttpRequestException">all other HTTP exceptions</exception>
        public async Task<InfluxDatabase> GetInfluxDBStructureAsync(string dbName)
        {
            var dbStructure = new InfluxDatabase(dbName);
            var fields = await QueryMultiSeriesAsync(dbName, "SHOW FIELD KEYS");
            foreach (var s in fields)
            {
                var measurement = new InfluxMeasurement(s.SeriesName);
                foreach (var e in s.Entries)
                    measurement.Fields.Add(e.FieldKey);
                dbStructure.Measurements.Add(measurement);
            }

            var tags = await QueryMultiSeriesAsync(dbName, "SHOW TAG KEYS");
            foreach (var t in tags)
            {
                var measurement = dbStructure.Measurements.FirstOrDefault(x => x.Name == t.SeriesName);
                foreach (var e in t.Entries)
                    measurement.Tags.Add(e.TagKey);
            }

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
            var response = await GetAsync(new Dictionary<string, string>() { { "q", String.Format("CREATE DATABASE {0}", dbName) } });
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                if (content.Contains("database already exists"))
                    throw new InvalidOperationException("database already exists");
                return true;
            }
            else if (response.StatusCode == HttpStatusCode.BadRequest)
                throw new ArgumentException("Invalid DB Name");

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
            ByteArrayContent requestContent = new ByteArrayContent(Encoding.UTF8.GetBytes(content));
            HttpResponseMessage response = await PostAsync(new Dictionary<string, string>() { { "db", dbName }, { "precision", precisionLiterals[(int)precision] } }, requestContent);

            if (response.StatusCode == HttpStatusCode.NoContent)
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
            var response = await GetAsync(new Dictionary<string, string>() { { "db", dbName }, { "q", measurementQuery } });
            if (response.StatusCode == HttpStatusCode.OK)
            {
                //var content = await response.Content.ReadAsStreamAsync();
                //DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(InfluxResponse));
                //var result = js.ReadObject(content) as InfluxResponse;

                var result = new JavaScriptSerializer().Deserialize<InfluxResponse>(await response.Content.ReadAsStringAsync());

                if (result?.Results?.Count > 1)
                    throw new ArgumentException("The query is resulting in Multi Series respone, which is not supported by this method");

                if (result?.Results?[0].Series?.Count > 1)
                    throw new ArgumentException("The query is resulting in Multi Series respone, which is not supported by this method");

                var series = result?.Results?[0].Series?[0];

                var results = new List<dynamic>();
                for (var row = 0; row < series?.Values?.Count; row++)
                {
                    dynamic entry = new ExpandoObject();
                    results.Add(entry);
                    for (var col = 0; col < series.Columns.Count; col++)
                    {
                        var header = char.ToUpper(series.Columns[col][0]) + series.Columns[col].Substring(1);

                        ((IDictionary<string, object>)entry).Add(header, series.Values[row][col]);
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

            ByteArrayContent requestContent = new ByteArrayContent(Encoding.UTF8.GetBytes(point.ConvertToInfluxLineProtocol()));
            HttpResponseMessage response = await PostAsync(new Dictionary<string, string>() { { "db", dbName },
                                                            { "precision", precisionLiterals[(int)point.Precision] },
                                                            {"rp",point.Retention==null?"default":point.Retention.Name} }, requestContent);

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var content = await response.Content.ReadAsStringAsync();
                //regex assumes error text from https://github.com/influxdata/influxdb/blob/master/models/points.go ParsePointsWithPrecision
                //fmt.Sprintf("'%s': %v", string(block[start:len(block)])
                List<string> parts; bool partialWrite;
                if (content.Contains("partial write"))
                {
                    if (content.Contains("\\n"))
                        parts = Regex.Matches(content.Substring(content.IndexOf("partial write:\\n") + 16), @"([\P{Cc}].*?) '([\P{Cc}].*?)':([\P{Cc}].*?)\\n").ToList();
                    else
                        parts = Regex.Matches(content.Substring(content.IndexOf("partial write:\\n") + 16), @"([\P{Cc}].*?) '([\P{Cc}].*?)':([\P{Cc}].*?)").ToList();
                    partialWrite = true;
                }
                else
                {
                    parts = Regex.Matches(content, @"{\""error"":""([9\P{Cc}]+) '([\P{Cc}]+)':([a-zA-Z0-9 ]+)").ToList();
                    partialWrite = false;
                }
                throw new InfluxDBException(partialWrite ? "Partial Write" : "Failed to Write", String.Format("{0}: {1} due to {2}", partialWrite ? "Partial Write" : "Failed to Write", parts[0], parts[2]), point);
                return false;
            }
            else if (response.StatusCode == HttpStatusCode.NoContent)
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
        /// <returns>True:Success, False:Failure, or partial failure</returns>
        /// Sets Saved property on InfluxDatapoint to true to successful points
        ///<exception cref="UnauthorizedAccessException">When Influx needs authentication, and no user name password is supplied or auth fails</exception>
        ///<exception cref="HttpRequestException">all other HTTP exceptions</exception>   
        public async Task<bool> PostPointsAsync(string dbName, IEnumerable<IInfluxDatapoint> Points)
        {
            int maxBatchSize = 255;
            bool finalResult = true, result;
            foreach (var group in Points.GroupBy(p => new { p.Precision, p.Retention?.Name }))
            {

                var pointsGroup = group.AsEnumerable().Select((point, index) => new { Index = index, Point = point })//get the index of each point
                          .GroupBy(x => x.Index / maxBatchSize) //chunk into smaller batches
                          .Select(x => x.Select(v => v.Point)); //get the points
                foreach (var points in pointsGroup)
                {
                    try
                    {
                        result = await PostPointsAsync(dbName, group.Key.Precision, group.Key.Name, points);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                    finalResult = result && finalResult;
                    if (result)
                    {
                        points.ToList().ForEach(p => p.Saved = true);
                    }
                }



            }

            return finalResult;
        }


        /// <summary>
        /// InfluxDB engine version
        /// </summary>
        public async Task<string> GetServerVersionAsync()
        {
            var querybaseUrl = new Uri(String.Format("{0}/ping", InfluxUrl));
            var builder = new UriBuilder(querybaseUrl);

            try
            {
                HttpResponseMessage response = await _client.GetAsync(builder.Uri);

                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    return response.Headers.GetValues("X-Influxdb-Version").FirstOrDefault();
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.BadGateway || (response.StatusCode == HttpStatusCode.InternalServerError && response.ReasonPhrase == "INKApi Error")) //502 Connection refused
                    throw new UnauthorizedAccessException("InfluxDB needs authentication. Check uname, pwd parameters");

            }
            catch (HttpRequestException e)
            {
                if (e.InnerException is WebException && e.InnerException.Message == "Unable to connect to the remote server")
                    throw new ServiceUnavailableException();
            }
            return "Unknown";
        }



        /// <summary>
        /// Gets the list of retention policies present in a DB
        /// </summary>
        /// <param name="dbName">Name of the database</param>
        /// <returns>List of InfluxRetentionPolicy objects</returns>
        public async Task<List<InfluxRetentionPolicy>> GetRetentionPoliciesAsync(string dbName)
        {
            var rawpolicies = await QueryDBAsync(dbName, "show retention policies on " + dbName);
            var policies = new List<InfluxRetentionPolicy>();

            foreach (var policy in rawpolicies)
            {
                var pol = new InfluxRetentionPolicy()
                {
                    DBName = dbName,
                    Name = policy.Name,
                    Duration = StringHelper.ParseDuration(policy.Duration),
                    IsDefault = (policy.Default == "true"),
                    ReplicaN = int.Parse(policy.ReplicaN),
                    Saved = true
                };
                try
                {
                    //supported from Influx 12 onwards
                    pol.ShardDuration = StringHelper.ParseDuration(policy.ShardGroupDuration);
                }
                catch (Exception) { }
                policies.Add(pol);
            }
            return policies;

        }



        /// <summary>
        /// Creates a retention policy
        /// </summary>
        /// <param name="policy">An instance of the Retention Policy, DBName, Name and Duration must be set</param>
        /// <returns>True: Success</returns>
        public async Task<bool> CreateRetentionPolicyAsync(InfluxRetentionPolicy policy)
        {
            var query = policy.GetCreateSyntax();
            if (query != null)
            {
                var response = await GetAsync(new Dictionary<string, string>() { { "q", query } });
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    policy.Saved = true;
                    return true;
                }
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                    throw new ArgumentException("Invalid Retention Policy");
            }
            return false;
        }

        /// <summary>
        /// Queries Influx DB and gets a time series data back. Ideal for fetching measurement values.
        /// The return list is of dynamics, and each element in there will have properties named after columns in series
        /// </summary>
        /// <param name="dbName">Name of the database</param>
        /// <param name="measurementQuery">Query text, Only results with single series are supported for now</param>
        /// <returns>List of InfluxSeries</returns>
        public async Task<List<InfluxSeries>> QueryMultiSeriesAsync(string dbName, string measurementQuery)
        {
            var response = await GetAsync(new Dictionary<string, string>() { { "db", dbName }, { "q", measurementQuery } });
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var results = new List<InfluxSeries>();
                
                //var content = await response.Content.ReadAsStreamAsync();
                //DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(InfluxResponse));
                //var rawResult = js.ReadObject(content) as InfluxResponse;

                var rawResult = new JavaScriptSerializer().Deserialize <InfluxResponse> (await response.Content.ReadAsStringAsync());

                if (rawResult.Results.Count > 1)
                    throw new ArgumentException("The query is resulting in a format, which is not supported by this method yet");

                if (rawResult.Results[0].Series != null)
                {

                    foreach (var series in rawResult.Results[0].Series)
                    {
                        var result = new InfluxSeries();
                        results.Add(result);
                        result.SeriesName = series.Name;
                        result.Tags = series.Tags;
                        result.Entries = new List<dynamic>();
                        for (var row = 0; row < series.Values.Count; row++)
                        {
                            dynamic entry = new ExpandoObject();
                            result.Entries.Add(entry);
                            for (var col = 0; col < series.Columns.Count; col++)
                            {
                                var header = char.ToUpper(series.Columns[col][0]) + series.Columns[col].Substring(1);

                                ((IDictionary<string, object>)entry).Add(header, series.Values[row][col]);
                            }
                        }
                    }
                }
                return results;
            }
            return null;
        }
    }

}
