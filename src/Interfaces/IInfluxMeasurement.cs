using System.Collections.Generic;

namespace AdysTech.InfluxDB.Client.Net
{
    public interface IInfluxMeasurement
    {   
        string Name { get; }
        ICollection<string> Fields { get; }
        ICollection<string> Tags { get; }

        int SeriesCount { get; }
        int PointsCount { get; }

        bool Deleted { get; }
    }
}