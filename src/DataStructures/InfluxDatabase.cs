using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public ICollection<IInfluxRetentionPolicy> RetentionPolicies
        {
            get
            {
                return new ReadOnlyCollection<IInfluxRetentionPolicy>(_rps.Keys.ToList());
            }
        }

        public IDictionary<IInfluxRetentionPolicy, ICollection<IInfluxMeasurement>> MeasurementHierarchy
        {
            get
            {
                return _rps;
            }
        }

        HashSet<IInfluxMeasurement> _measurements;
        Dictionary<IInfluxRetentionPolicy, ICollection<IInfluxMeasurement>> _rps;

        public InfluxDatabase(string name)
        {
            Name = name;
            _measurements = new HashSet<IInfluxMeasurement>();
            _rps = new Dictionary<IInfluxRetentionPolicy, ICollection<IInfluxMeasurement>>();
        }



    }
}
