using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AdysTech.InfluxDB.Client.Net
{
    public static class RegExHelper
    {
        public static List<string> ToList (this MatchCollection matches)
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
}

