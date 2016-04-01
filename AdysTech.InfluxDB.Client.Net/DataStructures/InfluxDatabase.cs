using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdysTech.InfluxDB.Client.Net
{
    public class InfluxDatabase : IInfluxDatabase
    {
        public string Name { get; internal set; }

        public ICollection<IInfluxMeasurement> Measurements
        {
            get
            {
                return _measurements;
            }
        }

        HashSet<IInfluxMeasurement> _measurements;
        public InfluxDatabase(string name)
        {
            Name = name;
            _measurements  = new HashSet<IInfluxMeasurement>();
        }

    }
}
