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
        /// The objects will have columns as Peoperties with their current values
        /// </summary>
        IReadOnlyList<dynamic> Entries { get; }
        
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
    }
}