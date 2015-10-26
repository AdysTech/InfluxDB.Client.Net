using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdysTech.InfluxDB.Client.Net
{
    public interface IInfluxDBClient
    {
        Task<List<String>> GetInfluxDBNamesAsync();
        Task<Dictionary<string, List<String>>> GetInfluxDBStructureAsync(string dbName);
        Task<bool> CreateDatabaseAsync(string dbName);
        Task<bool> PostValueAsync(string dbName, string measurement, long timestamp, TimePrecision precision, string tags, string field, double value);
        Task<bool> PostValuesAsync(string dbName, string measurement, long timestamp, TimePrecision precision, string tags, IDictionary<string,double> values);
        Task<bool> PostRawValueAsync(string dbName, TimePrecision precision, string content);
    }
}
