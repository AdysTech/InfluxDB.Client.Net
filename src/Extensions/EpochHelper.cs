using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdysTech.InfluxDB.Client.Net
{
    public static class EpochHelper
    {
        private static readonly DateTime Origin = new DateTime(new DateTime(1970, 1, 1).Ticks, DateTimeKind.Utc);

        public static long ToEpoch(this DateTime time, TimePrecision precision)
        {
            TimeSpan t = time - Origin;
            switch (precision)
            {
                case TimePrecision.Hours: return (long)t.TotalHours;
                case TimePrecision.Minutes: return (long)t.TotalMinutes;
                case TimePrecision.Seconds: return (long)t.TotalSeconds;
                case TimePrecision.Milliseconds: return (long)t.TotalMilliseconds;
                case TimePrecision.Microseconds: return (long)(t.TotalMilliseconds * 1000);
                case TimePrecision.Nanoseconds:
                default: return (long)t.Ticks * 100; //1 tick = 100 nano sec
            }
        }

        public static DateTime FromEpoch(this string time, TimePrecision precision)
        {
            long duration = long.Parse(time);
            DateTime t = Origin;
            switch (precision)
            {
                case TimePrecision.Hours: return t.AddHours(duration);
                case TimePrecision.Minutes: return t.AddMinutes(duration);
                case TimePrecision.Seconds: return t.AddSeconds(duration);
                case TimePrecision.Milliseconds: return t.AddMilliseconds(duration);
                case TimePrecision.Microseconds: return t.AddTicks(duration * TimeSpan.TicksPerMillisecond * 1000);
                case TimePrecision.Nanoseconds: return t.AddTicks(duration / 100); //1 tick = 100 nano sec
            }
            return t;
        }

    }
}
