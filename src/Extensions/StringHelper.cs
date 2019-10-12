using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AdysTech.InfluxDB.Client.Net
{

    public static class StringHelper
    {
        public static string Unescape(this string txt)
        {
            if (string.IsNullOrEmpty(txt)) { return txt; }
            StringBuilder retval = new StringBuilder(txt.Length);
            for (int ix = 0; ix < txt.Length;)
            {
                int jx = txt.IndexOf('\\', ix);
                if (jx < 0 || jx == txt.Length - 1) jx = txt.Length;
                retval.Append(txt, ix, jx - ix);
                if (jx >= txt.Length) break;
                switch (txt[jx + 1])
                {
                    case 'n': retval.Append('\n'); break;  // Line feed
                    case 'r': retval.Append('\r'); break;  // Carriage return
                    case 't': retval.Append('\t'); break;  // Tab
                    case ',': retval.Append(','); break;
                    case '"': retval.Append('"'); break;
                    case '=': retval.Append('='); break;
                    case '\\': retval.Append('\\'); break; // Don't escape
                    default:                                 // Unrecognized, copy as-is
                        retval.Append('\\').Append(txt[jx + 1]); break;
                }
                ix = jx + 2;
            }

            return retval.Replace("\\ ", " ").ToString();
        }

        /// <summary>
        /// Escapes the special charecters in string to match InfluxDB line protocol.
        /// For tag keys, tag values, and field keys always use a backslash character \ to escape: commas equal signs spaces
        /// For measurements always use a backslash character \ to escape: commas  spaces
        /// For string field values use a backslash character \ to escape: double quotes
        /// </summary>  
        /// <ref>https://docs.influxdata.com/influxdb/v1.6/write_protocols/line_protocol_reference/#special-characters
        /// </ref>
        /// <param name="val"></param>
        /// <param name="comma">ignore comma</param>
        /// <param name="equalSign">ignore Equal sign</param>
        /// <param name="doubleQuote">ignore  double quote</param>
        /// <param name="space">ignore space</param>
        /// <returns></returns>
        public static string EscapeChars(this string value, bool comma = false, bool equalSign = false, bool doubleQuote = false, bool space = false)
        {
            var val = new StringBuilder(value);
            // escape comma
            if (comma && value.Contains(','))
                val.Replace(",", "\\,");

            // escape space
            if (space && value.Contains(' '))
                val.Replace(" ", "\\ ");

            // escape double quotes
            if (doubleQuote && value.Contains('"'))
                val.Replace("\"", "\\\"");

            // escape equal sign
            if (equalSign && value.Contains('='))
                val.Replace("=", "\\=");

            if (value.Contains('\n'))
                val.Replace(" ", "\\n");
            //edge case, which will trigger Unbalanced Quotes exception in InfluxDB
            if (value.EndsWith("\\"))
                return val.ToString(0, val.Length - 1);

            return val.ToString();
        }

        public static TimeSpan ParseDuration(this string strDuration)
        {
            var durationLiterals = new string[] { "w", "d", "h", "m", "s", "ms", "u" };
            TimeSpan duration = TimeSpan.Zero;
            if (strDuration != "0")
            {

                var indexes = durationLiterals.Select(s => strDuration.IndexOf(s, 0)).ToArray();
                //handle only ms duration, and confusing with m and s with ms.
                if (indexes[3] == indexes[5] && indexes[4] > indexes[5])
                {
                    indexes[3] = indexes[5] = -1;
                }

                for (var index = 0; index < durationLiterals.Length; index++)
                {
                    if (indexes[index] > -1)
                    {
                        try
                        {
                            int val = 0;
                            var len = index == 0 || indexes[index - 1] == -1 ? indexes[index] : indexes[index] - indexes[index - 1] - 1;
                            var start = index == 0 || indexes[index - 1] == -1 ? 0 : indexes[index - 1] + 1;
                            if (int.TryParse(strDuration.Substring(start, len), out val))
                            {
                                switch (durationLiterals[index])
                                {
                                    case "w": duration += TimeSpan.FromDays(val * 7); break;
                                    case "d": duration += TimeSpan.FromDays(val); break;
                                    case "h": duration += TimeSpan.FromHours(val); break;
                                    case "m": duration += TimeSpan.FromMinutes(val); break;
                                    case "s": duration += TimeSpan.FromSeconds(val); break;
                                    case "ms": duration += TimeSpan.FromMilliseconds(val); break;
                                    case "u": duration += TimeSpan.FromMilliseconds(val / 1000); break;
                                }
                            }
                        }
                        catch (Exception)
                        {

                            throw;
                        }
                    }
                }

            }
            else
            {
                duration = TimeSpan.MaxValue;
            }

            return duration;
        }

    }
}
