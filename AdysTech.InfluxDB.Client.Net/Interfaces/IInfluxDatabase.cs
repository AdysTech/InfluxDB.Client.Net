using System.Collections.Generic;

namespace AdysTech.InfluxDB.Client.Net
{
    public interface IInfluxDatabase
    {
        ICollection<IInfluxMeasurement> Measurements { get; }
        string Name { get; }
    }
}