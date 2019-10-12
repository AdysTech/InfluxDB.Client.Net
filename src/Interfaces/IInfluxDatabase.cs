using System.Collections.Generic;

namespace AdysTech.InfluxDB.Client.Net
{
    public interface IInfluxDatabase
    {
        IDictionary<IInfluxRetentionPolicy, ICollection<IInfluxMeasurement>> MeasurementHierarchy{ get; }

        ICollection<IInfluxMeasurement> Measurements { get; }
        string Name { get; }

        bool Deleted { get;}
    }
}