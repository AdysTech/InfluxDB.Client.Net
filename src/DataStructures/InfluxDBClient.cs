//Copyright: Adarsha@AdysTech

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AdysTech.InfluxDB.Client.Net.DataContracts;
using Newtonsoft.Json;
using System.IO;

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
    public class InfluxDBClient : IInfluxDBClient, IDisposable
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

        private async Task<HttpResponseMessage> GetAsync(Dictionary<string, string> Query, HttpCompletionOption completion = HttpCompletionOption.ResponseContentRead)
        {
            var querybaseUrl = new Uri($"{InfluxUrl}/query?");
            var builder = new UriBuilder(querybaseUrl);

            builder.Query = await new FormUrlEncodedContent(Query).ReadAsStringAsync();
            try
            {
                HttpResponseMessage response = await _client.GetAsync(builder.Uri, completion);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return response;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.BadGateway || (response.StatusCode == HttpStatusCode.InternalServerError && response.ReasonPhrase == "INKApi Error")) //502 Connection refused
                    throw new UnauthorizedAccessException("InfluxDB needs authentication. Check uname, pwd parameters");
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    throw InfluxDBException.ProcessInfluxDBError(await response.Content.ReadAsStringAsync());
                }

            }
            catch (HttpRequestException e)
            {
                if (e.InnerException.Message == "Unable to connect to the remote server" ||
                    e.InnerException.Message == "A connection with the server could not be established" ||
                    e.InnerException.Message.StartsWith("The remote name could not be resolved:"))
                    throw new ServiceUnavailableException();
            }
            return null;
        }


        private async Task<HttpResponseMessage> PostAsync(Dictionary<string, string> EndPoint, ByteArrayContent requestContent)
        {

            var querybaseUrl = new Uri($"{InfluxUrl}/write?");
            var builder = new UriBuilder(querybaseUrl);


            builder.Query = await new FormUrlEncodedContent(EndPoint).ReadAsStringAsync();
            //if (requestContent.Headers.ContentType == null)
            //{
            //    requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            //    requestContent.Headers.ContentType.CharSet = "UTF-8";
            //}

            try
            {
                HttpResponseMessage response = await _client.PostAsync(builder.Uri, requestContent);

                if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.BadGateway || (response.StatusCode == HttpStatusCode.InternalServerError && response.ReasonPhrase == "INKApi Error")) //502 Connection refused
                    throw new UnauthorizedAccessException("InfluxDB needs authentication. Check uname, pwd parameters");

                return response;
            }
            catch (HttpRequestException e)
            {
                if (e.InnerException.Message == "Unable to connect to the remote server")
                    throw new ServiceUnavailableException();
            }
            return null;
        }

        private async Task<bool> PostPointsAsync(string dbName, TimePrecision precision, string retention, IEnumerable<IInfluxDatapoint> points)
        {
            Regex multiLinePattern = new Regex(@"([\P{Cc}].*?) '([\P{Cc}].*?)':([\P{Cc}].*?)\\n", RegexOptions.Compiled, TimeSpan.FromSeconds(5));
            Regex oneLinePattern = new Regex(@"{\""error"":""([9\P{Cc}]+) '([\P{Cc}]+)':([a-zA-Z0-9 ]+)", RegexOptions.Compiled, TimeSpan.FromSeconds(5));

            var line = new StringBuilder();
            foreach (var point in points)
                line.AppendFormat("{0}\n", point.ConvertToInfluxLineProtocol());
            //remove last \n
            line.Remove(line.Length - 1, 1);

            ByteArrayContent requestContent = new ByteArrayContent(Encoding.UTF8.GetBytes(line.ToString()));
            var endPoint = new Dictionary<string, string>() { { "db", dbName } };
            if (precision > 0)
                endPoint.Add("precision", precisionLiterals[(int)precision]);

            if (!String.IsNullOrWhiteSpace(retention))
                endPoint.Add("rp", retention);
            HttpResponseMessage response = await PostAsync(endPoint, requestContent);

            if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.BadGateway || (response.StatusCode == HttpStatusCode.InternalServerError && response.ReasonPhrase == "INKApi Error")) //502 Connection refused
                throw new UnauthorizedAccessException("InfluxDB needs authentication. Check uname, pwd parameters");
            //if(response.StatusCode==HttpStatusCode.NotFound)
            else if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var content = await response.Content.ReadAsStringAsync();
                //regex assumes error text from https://github.com/influxdata/influxdb/blob/master/models/points.go ParsePointsWithPrecision
                //fmt.Sprintf("'%s': %v", string(block[start:len(block)])
                List<string> parts = null; string l = "";

                if (content.Contains("partial write"))
                {
                    try
                    {
                        if (content.Contains("\\n"))
                            parts = multiLinePattern.Matches(content.Substring(content.IndexOf("partial write:\\n") + 16)).ToList();
                        else
                            parts = oneLinePattern.Matches(content.Substring(content.IndexOf("partial write:\\n") + 16)).ToList();

                        if (parts.Count == 0)
                            throw new InfluxDBException("Partial Write", new Regex(@"\""error\"":\""(.*?)\""").Match(content).Groups[1].Value);

                        if (parts[1].Contains("\\n"))
                            l = parts[1].Substring(0, parts[1].IndexOf("\\n")).Unescape();
                        else
                            l = parts[1].Unescape();
                    }
                    catch (InfluxDBException e)
                    {
                        throw e;
                    }
                    catch (Exception)
                    {

                    }

                    var point = points.Where(p => p.ConvertToInfluxLineProtocol() == l).FirstOrDefault();
                    if (point != null)
                        throw new InfluxDBException("Partial Write", $"Partial Write : {parts?[0]} due to {parts?[2]}", point);
                    else
                        throw new InfluxDBException("Partial Write", $"Partial Write : {parts?[0]} due to {parts?[2]}", l);
                }
                else
                {
                    throw InfluxDBException.ProcessInfluxDBError(content);
                }
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
        public InfluxDBClient(string InfluxUrl, string UserName, string Password) :
            this(InfluxUrl, UserName, Password, null)
        {
        }

        /// <summary>
        /// Creates the InfluxDB Client
        /// </summary>
        /// <param name="InfluxUrl">Url for the Inflex Server, e.g. http://localhost:8086</param>
        /// <param name="UserName">User name to authenticate InflexDB</param>
        /// <param name="Password">password</param>
        /// <param name="ClientHandler">HttpClientHandler which can be used to set the Web Proxy </param>
        public InfluxDBClient(string InfluxUrl, string UserName, string Password, HttpClientHandler ClientHandler)
        {
            try
            {
                this._influxUrl = InfluxUrl;
                this._influxDBUserName = UserName;
                this._influxDBPassword = Password;

                if (ClientHandler != null)
                    _client = new HttpClient(ClientHandler, true);
                else
                    _client = new HttpClient();

                if (!(String.IsNullOrWhiteSpace(InfluxDBUserName) && String.IsNullOrWhiteSpace(InfluxDBPassword)))
                    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                        Convert.ToBase64String(Encoding.UTF8.GetBytes($"{InfluxDBUserName}:{InfluxDBPassword}")));

                _client.DefaultRequestHeaders.ConnectionClose = false;
            }
            catch (Exception ex)
            {
                throw new InfluxDBException("Failed in ctor of InfluxDBClient", ex.ToString());
            }
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

            var dbs = await QueryMultiSeriesAsync(null, "SHOW DATABASES");

            foreach (var db in dbs?.FirstOrDefault()?.Entries)
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
        public async Task<IInfluxDatabase> GetInfluxDBStructureAsync(string dbName)
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
            var response = await GetAsync(new Dictionary<string, string>() { { "q", $"CREATE DATABASE \"{dbName}\"" } });
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                if (content.Contains("database already exists"))
                    throw new InvalidOperationException("database already exists");
                return true;
            }

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
            var endPoint = new Dictionary<string, string>() {
               { "db", dbName },
               { "precision", precisionLiterals[(int)point.Precision] }};
            if (!String.IsNullOrWhiteSpace(point.Retention?.Name))
                endPoint.Add("rp", point.Retention?.Name);
            HttpResponseMessage response = await PostAsync(endPoint, requestContent);

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                throw InfluxDBException.ProcessInfluxDBError(await response.Content.ReadAsStringAsync());
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
            foreach (var group in Points.Where(p => p.Retention == null || p.UtcTimestamp > DateTime.UtcNow - p.Retention.Duration).GroupBy(p => new { p.Precision, p.Retention?.Name }))
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
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
                    catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
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
            var querybaseUrl = new Uri($"{InfluxUrl}/ping");
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
                if (e.InnerException.Message == "Unable to connect to the remote server")
                    throw new ServiceUnavailableException();
            }
            return "Unknown";
        }



        /// <summary>
        /// Gets the list of retention policies present in a DB
        /// </summary>
        /// <param name="dbName">Name of the database</param>
        /// <returns>List of InfluxRetentionPolicy objects</returns>
        public async Task<List<IInfluxRetentionPolicy>> GetRetentionPoliciesAsync(string dbName)
        {
            var rawpolicies = await QueryMultiSeriesAsync(dbName, "show retention policies on " + dbName);
            var policies = new List<IInfluxRetentionPolicy>();

            foreach (var policy in rawpolicies.FirstOrDefault()?.Entries)
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
        public async Task<bool> CreateRetentionPolicyAsync(IInfluxRetentionPolicy policy)
        {
            var query = (policy as InfluxRetentionPolicy).GetCreateSyntax();
            if (query != null)
            {
                var response = await GetAsync(new Dictionary<string, string>() { { "q", query } });
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    (policy as InfluxRetentionPolicy).Saved = true;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Queries Influx DB and gets a time series data back. Ideal for fetching measurement values.
        /// The return list is of InfluxSeries, and each element in there will have properties named after columns in series
        /// </summary>
        /// <param name="dbName">Name of the database</param>
        /// <param name="measurementQuery">Query text, Only results with single series are supported for now</param>
        /// <param name="precision">epoch precision of the data set</param>
        /// <returns>List of InfluxSeries</returns>
        /// <seealso cref="InfluxSeries"/>
        public async Task<List<IInfluxSeries>> QueryMultiSeriesAsync(string dbName, string measurementQuery, TimePrecision precision = TimePrecision.Nanoseconds)
        {
            var response = await GetAsync(new Dictionary<string, string>() { { "db", dbName }, { "q", measurementQuery }, { "epoch", precisionLiterals[(int)precision] } });
            if (response == null) throw new ServiceUnavailableException();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var results = new List<IInfluxSeries>();
                var rawResult = JsonConvert.DeserializeObject<InfluxResponse>(await response.Content.ReadAsStringAsync());

                if (rawResult?.Results?.Count > 1)
                    throw new ArgumentException("The query is resulting in a format, which is not supported by this method yet");

                if (rawResult?.Results[0]?.Series != null)
                {
                    foreach (var series in rawResult?.Results[0]?.Series)
                    {
                        InfluxSeries result = GetInfluxSeries(precision, series);
                        results.Add(result);
                    }
                }
                return results;
            }
            return null;
        }


        /// <summary>
        /// Queries Influx DB and gets a time series data back. Ideal for fetching measurement values.
        /// The return list is of InfluxSeries, and each element in there will have properties named after columns in series
        /// THis uses Chunking support from InfluxDB. It returns results in streamed batches rather than as a single response 
        /// Responses will be chunked by series or by every ChunkSize points, whichever occurs first.
        /// </summary>
        /// <param name="dbName">Name of the database</param>
        /// <param name="measurementQuery">Query text, Only results with single series are supported for now</param>
        /// <param name="ChunkSize">Maximum Number of points in a chunk</param>
        /// <param name="precision">epoch precision of the data set</param>
        /// <returns>List of InfluxSeries</returns>
        /// <seealso cref="InfluxSeries"/>
        public async Task<List<IInfluxSeries>> QueryMultiSeriesAsync(string dbName, string measurementQuery, int ChunkSize, TimePrecision precision = TimePrecision.Nanoseconds)
        {
            var response = await GetAsync(new Dictionary<string, string>() {
                { "db", dbName },
                { "q", measurementQuery },
                {"chunked", "true" },
                {"chunk_size", ChunkSize.ToString() },
                { "epoch", precisionLiterals[(int)precision] } }, HttpCompletionOption.ResponseHeadersRead);
            if (response == null) throw new ServiceUnavailableException();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var results = new List<IInfluxSeries>();

                var stream = await response.Content.ReadAsStreamAsync();

                using (var reader = new StreamReader(stream))
                {
                    do
                    {
                        var str = await reader.ReadLineAsync();
                        var rawResult = JsonConvert.DeserializeObject<InfluxResponse>(str);
                        if (rawResult?.Results[0]?.Series != null)
                        {
                            foreach (var series in rawResult?.Results[0]?.Series)
                            {
                                InfluxSeries result = GetInfluxSeries(precision, series);
                                results.Add(result);
                            }
                        }
                        if (!rawResult.Results[0].Partial) break;
                    } while (!reader.EndOfStream);

                }
                return results;
            }
            return null;
        }

        /// <summary>
        /// Convert the Influx Series JSON objects to InfluxSeries
        /// </summary>
        /// <param name="precision"></param>
        /// <param name="series"></param>
        /// <returns></returns>
        private static InfluxSeries GetInfluxSeries(TimePrecision precision, Series series)
        {
            var result = new InfluxSeries()
            {
                HasEntries = false
            };

            result.SeriesName = series.Name;
            result.Tags = series.Tags;
            var entries = new List<dynamic>();
            for (var row = 0; row < series?.Values?.Count; row++)
            {
                result.HasEntries = true;
                dynamic entry = new ExpandoObject();
                entries.Add(entry);
                for (var col = 0; col < series.Columns.Count; col++)
                {
                    var header = char.ToUpper(series.Columns[col][0]) + series.Columns[col].Substring(1);

                    if (header == "Time")
                        ((IDictionary<string, object>)entry).Add(header, EpochHelper.FromEpoch(series.Values[row][col], precision));
                    else
                        ((IDictionary<string, object>)entry).Add(header, series.Values[row][col]);
                }
            }
            result.Entries = entries;
            return result;
        }


        /// <summary>
        /// Gets the list of Continuous Queries currently in efect
        /// </summary>
        /// <returns>List of InfluxContinuousQuery objects</returns>
        public async Task<List<IInfluxContinuousQuery>> GetContinuousQueriesAsync()
        {
            var cqPattern = new Regex(@"^CREATE CONTINUOUS QUERY (\S*) ON (\S*) (RESAMPLE (EVERY (\d\S)*)? ?(FOR (\d\S)*)?)? ?BEGIN ([\s\S]*GROUP BY time\((\d\S)\)[\s\S]*) END", RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromSeconds(10));
            //Show Continous Queries runs at a global scope, not just for a given DB.
            var rawCQList = await QueryMultiSeriesAsync(null, "SHOW CONTINUOUS QUERIES");
            var queries = new List<IInfluxContinuousQuery>();

            foreach (var dbEntry in rawCQList.Where(cq => cq.HasEntries == true))
            {
                foreach (var rawCQ in dbEntry.Entries)
                {
                    var cq = new InfluxContinuousQuery()
                    {
                        DBName = dbEntry.SeriesName,
                        Name = rawCQ.Name,
                        Saved = true
                    };
                    Match queryParts;
                    try
                    {
                        queryParts = cqPattern.Match(rawCQ.Query);
                        cq.ResampleFrequency = StringHelper.ParseDuration(queryParts.Groups[5].ToString());
                        cq.ResampleDuration = StringHelper.ParseDuration(queryParts.Groups[7].ToString());
                        cq.Query = queryParts.Groups[8].ToString();
                        cq.GroupByInterval = StringHelper.ParseDuration(queryParts.Groups[9].ToString());
                    }
#pragma warning disable CS0168 // The variable 'e' is declared but never used
                    catch (Exception e)
#pragma warning restore CS0168 // The variable 'e' is declared but never used
                    {
                        string query = rawCQ.Query.ToString();
                        var begin = query.IndexOf("BEGIN", StringComparison.CurrentCultureIgnoreCase) + 5;
                        cq.Query = query.Substring(begin, query.IndexOf(" END", StringComparison.CurrentCultureIgnoreCase));
                    }

                    queries.Add(cq);
                }
            }
            return queries;

        }
        /// <summary>
        /// Creates a Continuous Queries
        /// </summary>
        /// <param name="cq">An instance of the Continuous Query, DBName, Name, Query must be set</param>
        /// <returns>True: Success</returns>
        public async Task<bool> CreateContinuousQueryAsync(IInfluxContinuousQuery cq)
        {
            var query = (cq as InfluxContinuousQuery).GetCreateSyntax();
            if (query != null)
            {
                var response = await GetAsync(new Dictionary<string, string>() { { "q", query } });
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    (cq as InfluxContinuousQuery).Saved = true;
                    return true;
                }
            }
            return false;
        }

        public async Task<bool> DropContinuousQueryAsync(IInfluxContinuousQuery cq)
        {
            if (!cq.Saved)
                throw new ArgumentException("Continuous Query is not saved");
            var query = (cq as InfluxContinuousQuery).GetDropSyntax();
            if (query != null)
            {
                var response = await GetAsync(new Dictionary<string, string>() { { "q", query } });
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    (cq as InfluxContinuousQuery).Saved = false;
                    (cq as InfluxContinuousQuery).Deleted = true;
                    return true;
                }
            }
            return false;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        // The bulk of the clean-up code is implemented in Dispose(bool)
        protected virtual void Dispose(bool disposing)
        {
            _client?.Dispose();
            _client = null;
        }

    }
}