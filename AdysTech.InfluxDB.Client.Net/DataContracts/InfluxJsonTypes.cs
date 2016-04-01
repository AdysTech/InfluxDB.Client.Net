using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AdysTech.InfluxDB.Client.Net.DataContracts
{
    [DataContract]
    public class Series
    {
        [DataMember(Name="name")]
        public string SeriesName { get; set; }

        [DataMember(Name = "tags")]
        public Dictionary<string,string> Tags { get; set; }

        [DataMember (Name = "columns")]
        public List<string> ColumnHeaders { get; set; }

        [DataMember (Name = "values")]
        public List<List<string>> Values { get; set; }
    }

    [DataContract]
    public class Result
    {
        [DataMember (Name = "series")]
        public List<Series> Series { get; set; }
    }

    [DataContract]
    public class InfluxResponse
    {
        [DataMember (Name = "results")]
        public List<Result> Results { get; set; }
    }
}
