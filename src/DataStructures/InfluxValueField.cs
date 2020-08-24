using System;
using System.Globalization;
using System.Text;

namespace AdysTech.InfluxDB.Client.Net
{
    public class InfluxValueField : IInfluxValueField
    {
        public Type DataType => Value.GetType();

        public IComparable Value { get; }

        public InfluxValueField(IComparable val)
        {
            Value = val;
        }

        public override string ToString()
        {
            Type dataType = DataType;

            if (Value is string strValue)
            {
                // string needs escaping, but = is allowed in value --> Length limit 64KB.
                return $"\"{strValue.EscapeChars(doubleQuote: true)}\"";
            }
            else if (dataType == typeof(long) || dataType == typeof(int) || dataType == typeof(uint) || dataType == typeof(short) || dataType == typeof(ushort))
            {
                // Signed 64-bit integers (-9223372036854775808 to 9223372036854775807). Specify an integer with a trailing i on the number. Example: 1i.
                return $"{Value}i";
            }
            else if (Value is bool bValue)
            {
                // bool is okay with True or False
                return bValue.ToString();
            }
            else if (Value is double dblValue)
            {
                // double has to have a . as decimal seperator for Influx
                return String.Format(CultureInfo.GetCultureInfo("en-US"), "{0}", dblValue);
            }
            else if (Value is float flValue)
            {
                // double has to have a . as decimal seperator for Influx
                return String.Format(CultureInfo.GetCultureInfo("en-US"), "{0}", flValue);
            }
            else if (Value is DateTime dtValue)
            {
                // Unix nanosecond timestamp. Specify alternative precisions with the HTTP API. The minimum valid timestamp is -9223372036854775806 or 1677-09-21T00:12:43.145224194Z. The maximum valid timestamp is 9223372036854775806 or 2262-04-11T23:47:16.854775806Z.
                // InfluxDB does not support a datetime type for fields or tags
                // Convert datetime to UNIX long
                return dtValue.ToEpoch(TimePrecision.Milliseconds).ToString();
            }
            else if (Value is TimeSpan tsValue)
            {
                return $"{tsValue.TotalMilliseconds}i";
            }

            return ($"{dataType} is not supported by this library at this point");
        }

        public int CompareTo(object obj)
        {
            if (obj is IInfluxValueField value)
            {
                return CompareTo(value);
            }

            throw new ArgumentException("Not comparable");
        }

        public int CompareTo(IInfluxValueField other)
        {
            if (other.DataType == DataType)
            {
                return Value.CompareTo(other.Value);
            }

            throw new ArgumentException("Not comparable");
        }
    }
}
