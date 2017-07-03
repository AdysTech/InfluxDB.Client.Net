using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdysTech.InfluxDB.Client.Net.Tests
{
    static class DataGen
    {
        private static readonly Random random = new Random();
        private static readonly object syncLock = new object();

        public static int RandomNumber(int min, int max)
        {
            lock (syncLock)
            { // synchronize
                return random.Next(min, max);
            }
        }

        public static int RandomInt()
        {
            lock (syncLock)
            { // synchronize
                return random.Next();
            }
        }

        public static int RandomInt(int Max)
        {
            lock (syncLock)
            { // synchronize
                return random.Next(Max);
            }
        }

        public static double RandomDouble()
        {
            lock (syncLock)
            { // synchronize
                return random.NextDouble();
            }
        }

        public static string RandomString()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 ,=/\"";
            var stringChars = new StringBuilder(16, 16);

            for (int i = 0; i < stringChars.Capacity; i++)
            {
                lock (syncLock)
                {
                    stringChars.Append(chars[random.Next(chars.Length)]);
                }
            }
            return stringChars.ToString(); ;
        }

        public static DateTime RandomDate(DateTime from, DateTime to)
        {
            lock (syncLock)
            {
                return from.AddTicks((long)(random.NextDouble() * (to.Ticks - from.Ticks)));
            }
        }
    }

}
