using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AdysTech.InfluxDB.Client.Net
{
    public static class EpochHelper
    {
        private static readonly DateTime Origin = new DateTime (1970, 1, 1);

        public static long ToEpoch(this DateTime time, TimePrecision precision)
        {
            TimeSpan t = time - Origin;
            switch ( precision )
            {
                case TimePrecision.Hours: return (long) t.TotalHours;
                case TimePrecision.Minutes: return (long) t.TotalMinutes;
                case TimePrecision.Seconds: return (long) t.TotalSeconds;
                case TimePrecision.Milliseconds: return (long) t.TotalMilliseconds;
                case TimePrecision.Microseconds: return (long) t.Ticks / ( TimeSpan.TicksPerMillisecond * 1000 );
                case TimePrecision.Nanoseconds: return (long) t.Ticks * 100; //1 tick = 100 nano sec
            }
            return 0;
        }
    }

    public static class RegExHelper
    {
        public static List<string> ToList(this MatchCollection matches)
        {
            return matches.Cast<Match> ()
                // flatten to single list
                .SelectMany (o =>
                    // extract captured results
                    o.Groups.Cast<Capture> ()
                        // don't need the pattern
                    .Skip (1)
                    .Select (c => c.Value)).ToList ();
        }
    }

    public static class StringHelper
    {
        public static string Unescape(this string txt)
        {
            if ( string.IsNullOrEmpty (txt) ) { return txt; }
            StringBuilder retval = new StringBuilder (txt.Length);
             for ( int ix = 0; ix < txt.Length; )
            {
                int jx = txt.IndexOf ('\\', ix);
                if ( jx < 0 || jx == txt.Length - 1 ) jx = txt.Length;
                retval.Append (txt, ix, jx - ix);
                if ( jx >= txt.Length ) break;
                switch ( txt[jx + 1] )
                {
                    case 'n': retval.Append ('\n'); break;  // Line feed
                    case 'r': retval.Append ('\r'); break;  // Carriage return
                    case 't': retval.Append ('\t'); break;  // Tab
                    case ',': retval.Append (','); break;
                    case '"': retval.Append ('"'); break;
                    case '=': retval.Append ('='); break;
                    case '\\': retval.Append ('\\'); break; // Don't escape
                    default:                                 // Unrecognized, copy as-is
                        retval.Append ('\\').Append (txt[jx + 1]); break;
                }
                ix = jx + 2;
            }

            return retval.ToString ();
        }

    }
}
