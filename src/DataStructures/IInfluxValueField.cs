using System;

namespace AdysTech.InfluxDB.Client.Net
{
    public interface IInfluxValueField : IComparable, IComparable<IInfluxValueField>
    {
        Type DataType { get; }

        IComparable Value { get; }

        string ToString();
    }
}