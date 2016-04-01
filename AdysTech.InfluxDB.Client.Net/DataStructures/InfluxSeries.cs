using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdysTech.InfluxDB.Client.Net
{
    public class InfluxSeries
    {
        public string SeriesName { get; internal set; }
        public Dictionary<string, string> Tags { get; internal set; }
        public List<dynamic> Entries { get; internal set; }
    }
}
