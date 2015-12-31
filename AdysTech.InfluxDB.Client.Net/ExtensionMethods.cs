using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdysTech.InfluxDB.Client.Net
{
    public static class EpochHelper
    {
        private static readonly DateTime Origin = new DateTime (1970, 1, 1);

        public static long ToEpoch(this DateTime time, TimePrecision precision)
        {
            TimeSpan t = time - Origin;
            switch ( precision )
            {
                case TimePrecision.Hours: return (long) t.TotalHours;
                case TimePrecision.Minutes: return (long) t.TotalMinutes;
                case TimePrecision.Seconds: return (long) t.TotalSeconds;
                case TimePrecision.Milliseconds: return (long) t.TotalMilliseconds;
                case TimePrecision.Microseconds: return (long) t.Ticks * ( TimeSpan.TicksPerMillisecond / 1000 );
                case TimePrecision.Nanoseconds: return (long) t.Ticks * 100; //1 tick = 100 nano sec
            }
            return 0;
        }
    }
}
