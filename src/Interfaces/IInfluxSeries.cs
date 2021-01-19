using System.Collections.Generic;

namespace AdysTech.InfluxDB.Client.Net
{
    /// <summary>
    /// Represents the results returned by Query end point of the InfluxDB engine
    /// </summary>
    public interface IInfluxSeries
    {
        /// <summary>	        
        /// Read only List of ExpandoObjects (in the form of dynamic) representing the entries in the query result 	                
        /// The objects will have columns as Properties with their current values
        /// </summary>
        IReadOnlyList<dynamic> Entries { get; }

        /// <summary>
        /// Read only List of Dictionaries representing the entries in the query result 
        /// The objects will have columns as Properties with their current values
        /// </summary>
        IReadOnlyList<Dictionary<string, object>> EntriesAsDictionary { get; }
        
        /// <summary>
        /// Indicates whether this Series has any entries or not
        /// </summary>
        bool HasEntries { get; }

        /// <summary>
        /// Name of the series. Usually its MeasurementName in case of select queries
        /// </summary>
        string SeriesName { get; }

        /// <summary>
        /// Dictionary of tags, and their respective values.
        /// </summary>
        IReadOnlyDictionary<string, string> Tags { get; }

        /// <summary>
        /// True if the influx query was answered with a partial response due to e.g. exceeding a configured
        /// max-row-limit in the InfluxDB. As we don't know which series was truncated by InfluxDB, all series
        /// of the response will be flagged with Partial=true.
        /// </summary>
        bool Partial { get; set; }
    }

    /// <summary>
    /// Represents the results returned by Query end point of the InfluxDB engine, supporting strong typing
    /// </summary>
    public interface IInfluxSeries<T>
    {
        /// <summary>
        /// Read only List of T representing the entries in the query result 
        /// The objects will have been deserialized from the results
        /// </summary>
        IReadOnlyList<T> Entries { get; }

        /// <summary>
        /// Indicates whether this Series has any entries or not
        /// </summary>
        bool HasEntries { get; }

        /// <summary>
        /// Name of the series. Usually its MeasurementName in case of select queries
        /// </summary>
        string SeriesName { get; }

        /// <summary>
        /// Dictionary of tags, and their respective values.
        /// </summary>
        IReadOnlyDictionary<string, string> Tags { get; }

        /// <summary>
        /// True if the influx query was answered with a partial response due to e.g. exceeding a configured
        /// max-row-limit in the InfluxDB. As we don't know which series was truncated by InfluxDB, all series
        /// of the response will be flagged with Partial=true.
        /// </summary>
        bool Partial { get; set; }
    }
}