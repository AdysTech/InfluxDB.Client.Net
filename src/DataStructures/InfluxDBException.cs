using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AdysTech.InfluxDB.Client.Net
{
    [Serializable]
    public class InfluxDBException : InvalidOperationException
    {
        static Regex _errorLinePattern = new Regex (@"{\""error\"":\""([\S\s]+): ([\S\s]+)\""}", RegexOptions.Compiled, TimeSpan.FromSeconds (5));

        public string Reason { get; private set; }
        public IInfluxDatapoint FailedPoint { get; private set; }
        public string FailedLine { get; private set; }
        public InfluxDBException (string reason, string message)
            : base (message)
        {
            Reason = reason;
        }

        public InfluxDBException (string reason, string message, IInfluxDatapoint point)
            : this (reason, message)
        {
            FailedPoint = point;
        }

        public InfluxDBException (string reason, string message, string line)
            : this (reason, message)
        {
            FailedLine = line;
        }

        [SecurityPermission (SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData (SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException ("info");

            info.AddValue ("Reason", Reason);
            base.GetObjectData (info, context);
        }

        public static InfluxDBException ProcessInfluxDBError (string InfluxError)
        {
            var parts = _errorLinePattern.Match (InfluxError)?.Groups;
            //incase of points writes, Influx Error contains whole line protocol string. This needs to be removed to make error message readable.
            var reason = parts?[1]?.ToString ();
            if (reason.Contains ("'"))
            {
                var n = reason.IndexOf ("'");
                reason = reason.Substring (n, reason.LastIndexOf ("'") - n);
            }
            var message = (parts?[2]?.ToString ());
            if (message.Contains ("\\\""))
                message = message.Replace ("\\\"", "'");
            return new InfluxDBException (reason, message);
        }
    }
}
