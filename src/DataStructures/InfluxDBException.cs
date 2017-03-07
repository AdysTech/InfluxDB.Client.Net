using System;
using System.Text.RegularExpressions;

namespace AdysTech.InfluxDB.Client.Net
{
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
