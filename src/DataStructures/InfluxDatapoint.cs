using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

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
        public Dictionary<string, string> Tags { get; internal set; }

        /// <summary>
        /// The key-value pair in InfluxDB’s data structure that records metadata and the actual data value.
        /// Fields are required in InfluxDB’s data structure and they are not indexed.
        /// </summary>
        public Dictionary<string, T> Fields { get; internal set; }

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

        public IInfluxRetentionPolicy Retention { get; set; }

        public InfluxDatapoint()
        {
            Type itemType = typeof(T);
            if (itemType == typeof(long) ||
                itemType == typeof(decimal) ||
                itemType == typeof(double) ||
                itemType == typeof(int) ||
                itemType == typeof(bool) ||
                itemType == typeof(string) ||
                typeof(IInfluxValueField).IsAssignableFrom(itemType))
            {
                Tags = new Dictionary<string, string>();
                Fields = new Dictionary<string, T>();
                Saved = false;
            }
            else
            {
                throw new ArgumentException(itemType.Name + " is not supported by InfluxDB!");
            }
        }

        /// <summary>
        /// Initializes tags with a preexisitng dictionary
        /// </summary>
        /// <param name="tags">Dictionary of tag/value pairs</param>
        /// <returns>True:Success, False:Failure</returns>
        public bool InitializeTags(IDictionary<string, string> tags)
        {
            if (Tags.Count > 0)
                throw new InvalidOperationException("Tags can be initialized only when the collection is empty");
            try
            {
                Tags = new Dictionary<string, string>(tags);
                return true;
            }
            catch (Exception)
            {
                Tags = new Dictionary<string, string>();
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
            if (Fields.Count > 0)
                throw new InvalidOperationException("Fields can be initialized only when the collection is empty");
            try
            {
                Fields = new Dictionary<string, T>(fields);
                return true;
            }
            catch (Exception)
            {
                Fields = new Dictionary<string, T>();
                return false;
            }
        }

        /// <summary>
        /// Returns the string representing the point in InfluxDB Line protocol
        /// </summary>
        /// <returns></returns>
        /// <see cref="https://docs.influxdata.com/influxdb/v0.12/write_protocols/line/"/>
        public string ConvertToInfluxLineProtocol()
        {
            var line = new StringBuilder();
            ConvertToInfluxLineProtocol(line);
            return line.ToString();
        }

        public void ConvertToInfluxLineProtocol(StringBuilder line)

        {
            if (Fields.Count == 0)
                throw new InvalidOperationException("InfluxDB needs atleast one field in a line");
            if (String.IsNullOrWhiteSpace(MeasurementName))
                throw new InvalidOperationException("InfluxDB needs a measurement name to accept a point");


            line.AppendFormat("{0}", MeasurementName.EscapeChars(comma: true, space: true));

            if (Tags.Count > 0)
            {
                Tags.ToList().ForEach(t =>
                {
                    line.AppendFormat(",{0}={1}", t.Key.EscapeChars(comma: true, equalSign: true, space: true), t.Value.EscapeChars(comma: true, equalSign: true, space: true));
                });
            }

            var tType = typeof(T);
            string fields;
            line.Append(" ");

            if (tType == typeof(string))
            {
                //string needs escaping, but = is allowed in value
                Fields.ToList().ForEach(v =>
                {
                    line.AppendFormat("{0}=\"{1}\",", v.Key.EscapeChars(comma: true, equalSign: true, space: true), v.Value.ToString().EscapeChars(doubleQuote: true));
                });
                line.Remove(line.Length - 1, 1);
            }
            else if (tType == typeof(long) || tType == typeof(int))
            {
                //int needs i suffix
                Fields.ToList().ForEach(v =>
                {
                    line.AppendFormat("{0}={1}i,", v.Key.EscapeChars(comma: true, equalSign: true, space: true), v.Value);
                });
                line.Remove(line.Length - 1, 1);
            }

            else if (tType == typeof(bool))
            {
                //bool is okay with True or False
                Fields.ToList().ForEach(v =>
                {
                    line.AppendFormat("{0}={1},", v.Key.EscapeChars(comma: true, equalSign: true, space: true), v.Value);
                });
                line.Remove(line.Length - 1, 1);
            }

            else if (tType == typeof(double))
            {
                //double has to have a . as decimal seperator for Influx
                Fields.ToList().ForEach(v =>
                {
                    line.AppendFormat("{0}={1},", v.Key.EscapeChars(comma: true, equalSign: true, space: true), String.Format(CultureInfo.GetCultureInfo("en-US"), "{0}", v.Value));
                });
                line.Remove(line.Length - 1, 1);
            }
            else if (typeof(IInfluxValueField).IsAssignableFrom(tType))
            {
                //fields = String.Join(",", Fields.Select(v => new StringBuilder().AppendFormat("{0}={1}", v.Key.EscapeChars(comma: true, equalSign: true, space: true), v.Value.ToString())));
                Fields.ToList().ForEach(v =>
                {
                    line.AppendFormat("{0}={1},", v.Key.EscapeChars(comma: true, equalSign: true, space: true), v.Value.ToString());
                });
                line.Remove(line.Length - 1, 1);
            }

            else
                throw new ArgumentException(tType + " is not supported by this library at this point");

            line.AppendFormat(" {0}", UtcTimestamp != DateTime.MinValue ? UtcTimestamp.ToEpoch(Precision) : DateTime.UtcNow.ToEpoch(Precision));

        }
    }
}