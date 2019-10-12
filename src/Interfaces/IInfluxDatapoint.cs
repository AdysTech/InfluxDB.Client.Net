using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdysTech.InfluxDB.Client.Net
{
    /// <summary>
    /// Represents a single Point (collection of fields) in a series. 
    /// Each point is uniquely identified by its series and timestamp.
    /// </summary>
    public interface IInfluxDatapoint
    {

        /// <summary>
        /// Gets/Sets the InfluxDB retention policy that the point should be stored under
        /// </summary>
        IInfluxRetentionPolicy Retention { get; set; }

        /// <summary>
        /// Gets/Sets the InfluxDB measurement name
        /// </summary>
        String MeasurementName { get; set; }

        /// <summary>
        /// Dictionary storing all Tag/Value combinations
        /// </summary>
        Dictionary<string, string> Tags { get; }

        /// <summary>
        /// Time precision of the point, allowed are Hour->Nano second
        /// </summary>
        TimePrecision Precision { get; set; }

        /// <summary>
        /// Timestamp for the point, will be converted to Epoch, expcted to be in UTC
        /// </summary>
        DateTime UtcTimestamp { get; set; }

        /// <summary>
        /// Indicates whether a point got saved to InfluxDB successfully
        /// </summary>
        bool Saved { get; set; }

        

        /// <summary>
        /// Returns the string representing the point in InfluxDB Line protocol
        /// </summary>
        /// <returns></returns>
        /// <see cref="https://influxdb.com/docs/v0.9/write_protocols/write_syntax.html"/>
        string ConvertToInfluxLineProtocol();
        
        /// <summary>
        /// Appends the points Influx line protocol representation to input StringBuilder
        /// </summary>
        /// <returns></returns>
        void ConvertToInfluxLineProtocol(StringBuilder line);

    }

    /// <summary>
    /// Type specific dictionary of field values. 
    /// </summary>
    /// <typeparam name="T">Allowed: bool, string, int, decimal, double</typeparam>
    public interface IInfluxDatapoint<T> where T : IComparable, IComparable<T>
    {
        /// <summary>
        /// The key-value pair in InfluxDB’s data structure that records metadata and the actual data value. 
        /// Fields are required in InfluxDB’s data structure and they are not indexed.
        /// </summary>
        Dictionary<string, T> Fields { get; }
    }
}
