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
            return $"{Name} : Fileds-{Fields.Count}, Tags-{Tags.Count}, Series-{SeriesCount}, Points-{PointsCount}";
        }

        int _seriesCount = -1;
        int _pointsCount = -1;

        /// <summary>
        /// Gets the number of series in the given measurement. -1 indicates invalid data.
        /// </summary>
        public int SeriesCount
        {
            get { return _seriesCount; }
            internal set { _seriesCount = value; }
        }

        /// <summary>
        /// Gets number of points in the given measurement
        /// </summary>
        public int PointsCount
        {
            get { return _pointsCount; }
            internal set { _pointsCount = value; }
        }

    }
}
