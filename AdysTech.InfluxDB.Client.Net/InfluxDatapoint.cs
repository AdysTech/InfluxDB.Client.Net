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
    /// <typeparam name="T">Type of the filed value, Allowed - boolean, string, integer, decimal and double </typeparam>
    public class InfluxDatapoint<T> : IInfluxDatapoint, IInfluxDatapoint<T> where T : IComparable, IComparable<T>
    {
        /// <summary>
        /// Gets/Sets the InfluxDB measurement name
        /// </summary>
        public String MeasurementName { get; set; }

        /// <summary>
        /// Dictionary storing all Tag/Value combinations
        /// </summary>
        public Dictionary<string, string> Tags { get; private set; }

        /// <summary>
        /// The key-value pair in InfluxDB’s data structure that records metadata and the actual data value. 
        /// Fields are required in InfluxDB’s data structure and they are not indexed.
        /// </summary>
        public Dictionary<string, T> Fields { get; private set; }

        /// <summary>
        /// Timestamp for the point, will be converted to Epoch, expcted to be in UTC
        /// </summary>
        public DateTime UtcTimestamp { get; set; }

        /// <summary>
        /// Time precision of the point, allowed are Hour->Nano second
        /// </summary>
        public TimePrecision Precision { get; set; }

        /// <summary>
        /// Indicates whether a point got saved to InfluxDB successfully
        /// </summary>
        public bool Saved { get; set; }

        public InfluxDatapoint()
        {
            Type itemType = typeof (T);
            if ( itemType == typeof (int) || itemType == typeof (decimal) || itemType == typeof (double) || itemType == typeof (bool) || itemType == typeof (string) )
            {
                Tags = new Dictionary<string, string> ();
                Fields = new Dictionary<string, T> ();
                Saved = false;
            }
            else
            {
                throw new ArgumentException (itemType.Name + " is not supported by InfluxDB!");
            }
        }

        /// <summary>
        /// Initializes tags with a preexisitng dictionary
        /// </summary>
        /// <param name="tags">Dictionary of tag/value pairs</param>
        /// <returns>True:Success, False:Failure</returns>
        public bool InitializeTags(IDictionary<string, string> tags)
        {
            if ( Tags.Count > 0 )
                throw new InvalidOperationException ("Tags can be initialized only when the collection is empty");
            try
            {
                Tags = new Dictionary<string, string> (tags);
                return true;
            }
            catch ( Exception )
            {
                Tags = new Dictionary<string, string> ();
                return false;
            }
        }

        /// <summary>
        /// Initializes fields with a preexisting dictionary
        /// </summary>
        /// <param name="fields">Dictionary of Field/Value pairs</param>
        /// <returns>True:Success, False:Failure</returns>
        public bool InitializeFields(IDictionary<string, T> fields)
        {
            if ( Fields.Count > 0 )
                throw new InvalidOperationException ("Fields can be initialized only when the collection is empty");
            try
            {
                Fields = new Dictionary<string, T> (fields);
                return true;
            }
            catch ( Exception )
            {
                Fields = new Dictionary<string, T> ();
                return false;
            }
        }

        /// <summary>
        /// Returns the string representing the point in InfluxDB Line protocol
        /// </summary>
        /// <returns></returns>
        /// <see cref="https://influxdb.com/docs/v0.9/write_protocols/write_syntax.html"/>
        public string ConvertToInfluxLineProtocol()
        {
            if ( Fields.Count == 0 )
                throw new InvalidOperationException ("InfluxDB needs atleast one field in a line");
            if ( String.IsNullOrWhiteSpace (MeasurementName) )
                throw new InvalidOperationException ("InfluxDB needs a measurement name to accept a point");

            var line = new StringBuilder ();
            line.AppendFormat ("{0}", MeasurementName);

            if ( Tags.Count > 0 )
                line.AppendFormat (",{0}", String.Join (",", Tags.Select (t => new StringBuilder ().AppendFormat ("{0}={1}", EscapeChars (t.Key), EscapeChars (t.Value)))));

            var tType = typeof (T);
            string fields;

            if ( tType == typeof (string) )
                //string needs escaping, but = is allowed in value
                fields = String.Join (",", Fields.Select (v => new StringBuilder ().AppendFormat ("{0}=\"{1}\"", EscapeChars (v.Key), EscapeChars (v.Value.ToString (), false))));
            else if ( tType == typeof (int) )
                //int needs i suffix
                fields = String.Join (",", Fields.Select (v => new StringBuilder ().AppendFormat ("{0}={1}i", EscapeChars (v.Key), v.Value)));
            else if ( tType == typeof (bool) )
                //bool is okay with True or False
                fields = String.Join (",", Fields.Select (v => new StringBuilder ().AppendFormat ("{0}={1}", EscapeChars (v.Key), v.Value)));
            else
                ////double has to have a . as decimal seperator for Influx
                fields = String.Join (",", Fields.Select (v => new StringBuilder ().AppendFormat ("{0}={1}", EscapeChars (v.Key), String.Format (System.Globalization.CultureInfo.GetCultureInfo ("en-US"), "{0}", v.Value))));


            line.AppendFormat (" {0} {1}", fields, UtcTimestamp != DateTime.MinValue ? UtcTimestamp.ToEpoch (Precision) : DateTime.UtcNow.ToEpoch (Precision));

            return line.ToString ();
        }

        private string EscapeChars(string val, bool escapeEqualSign = true)
        {
            if ( val.Contains (',') )
                val = val.Replace (",", "\\,");
            if ( val.Contains (' ') )
                val = val.Replace (" ", "\\ ");
            if ( escapeEqualSign && val.Contains ('=') )
                val = val.Replace ("=", "\\=");
            //edge case, which will trigger Unbalanced Quotes exception in InfluxDB
            if ( val.EndsWith ("\\") )
                val = val.Substring (0, val.Length - 1);

            return val;
        }



    }
}
