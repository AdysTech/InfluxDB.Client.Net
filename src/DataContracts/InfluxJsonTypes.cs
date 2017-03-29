using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AdysTech.InfluxDB.Client.Net.DataContracts
{
    [DataContract]
    public class Series
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "tags")]
        public Dictionary<string,string> Tags { get; set; }

        [DataMember(Name = "columns")]
        public List<string> Columns { get; set; }

        [DataMember(Name = "values")]
        public List<List<string>> Values { get; set; }

        [DataMember(Name = "partial")]
        public bool Partial { get; set; }

    }

    [DataContract]
    public class Result
    {
        [DataMember(Name = "statement_id")]
        public int StatementID { get; set; }

        [DataMember(Name = "series")]
        public List<Series> Series { get; set; }

        [DataMember(Name = "partial")]
        public bool Partial { get; set; }

    }

    [DataContract]
    public class InfluxResponse
    {
        [DataMember(Name = "results")]
        public List<Result> Results { get; set; }
    }
}
