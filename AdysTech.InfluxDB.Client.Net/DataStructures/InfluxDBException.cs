using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdysTech.InfluxDB.Client.Net
{
    public class InfluxDBException : InvalidOperationException
    {
        public string Reason { get; private set; }
        public IInfluxDatapoint FailedPoint { get; private set; }
        public string FailedLine { get; private set; }
        public InfluxDBException(string reason, string message)
            : base (message)
        {
            Reason = reason;
        }

        public InfluxDBException(string reason, string message, IInfluxDatapoint point)
            : this (reason, message)
        {
            FailedPoint = point;
        }

        public InfluxDBException(string reason, string message, string line)
            : this (reason, message)
        {
            FailedLine = line;
        }
    }
}
