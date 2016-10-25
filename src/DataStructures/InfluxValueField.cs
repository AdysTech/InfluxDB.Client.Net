using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdysTech.InfluxDB.Client.Net
{
    public class InfluxValueField : IComparable, IComparable<InfluxValueField>
    {
        public Type DataType { get { return Value.GetType(); } }
        public IComparable Value { get; private set; }

        public InfluxValueField(IComparable val)
        {
            Value = val;
        }

        public override string ToString()
        {
            if (DataType == typeof(string))
                //string needs escaping, but = is allowed in value
                return new StringBuilder().AppendFormat("\"{0}\"", Value.ToString().EscapeChars(false)).ToString();
            else if (DataType == typeof(long) || DataType == typeof(int))
                //int needs i suffix
                return new StringBuilder().AppendFormat("{0}i", Value.ToString()).ToString();
            else if (DataType == typeof(bool))
                //bool is okay with True or False
                return Value.ToString();
            else if (DataType == typeof(double))
                ////double has to have a . as decimal seperator for Influx
                return String.Format(new CultureInfo("en-US"), "{0}", (double)Value);
            else
                throw new ArgumentException(DataType + " is not supported by this library at this point");

        }

        public int CompareTo(object obj)
        {
            if (!(obj is InfluxValueField))
                throw new ArgumentException("Not comparable");
            return CompareTo(obj as InfluxValueField);

        }

        public int CompareTo(InfluxValueField other)
        {
            if (other.DataType != DataType)
                throw new ArgumentException("Not comparable");
            return Value.CompareTo(other.Value);
        }
    }
}
