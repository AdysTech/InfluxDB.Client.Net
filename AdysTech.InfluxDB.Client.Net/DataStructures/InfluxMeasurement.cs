using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdysTech.InfluxDB.Client.Net
{
    public class InfluxMeasurement : IInfluxMeasurement
    {
        public string Name { get; private set; }

        public ICollection<string> Fields
        {
            get
            {
                return _fields;
            }
        }

        public ICollection<string> Tags
        {
            get
            {
                return _tags;
            }
        }

        HashSet<string> _tags;
        HashSet<string> _fields;

        public InfluxMeasurement(string name)
        {
            Name = name;
            _fields = new HashSet<string>();
            _tags = new HashSet<string>();
        }

        public override string ToString()
        {
            return String.Format("{0} : Fileds-{1}, Tags-{2}", Name, Fields.Count, Tags.Count);
        }

    }
}
