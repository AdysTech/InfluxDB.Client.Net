using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdysTech.InfluxDB.Client.Net.Tests
{
    //https://github.com/dotnet/coreclr/issues/2317#issuecomment-229965996
    public static class DateTimeExtensions
    {
        public static string ToShortDateString (this DateTime dateTime) => dateTime.ToString ("d");
        public static string ToShortTimeString (this DateTime dateTime) => dateTime.ToString ("t");
        public static string ToLongDateString (this DateTime dateTime) => dateTime.ToString ("D");
        public static string ToLongTimeString (this DateTime dateTime) => dateTime.ToString ("T");
    }

    public static class AssertEx
    {
        public static async Task<TException>
           ThrowsAsync<TException> (Func<Task> action,
        bool allowDerivedTypes = true) where TException : Exception
        {
            try
            {
                await action ();
            }
            catch (Exception ex)
            {
                if (allowDerivedTypes && !(ex is TException))
                    throw new Exception ("Delegate threw exception of type " +
                    ex.GetType ().Name + ", but " + typeof (TException).Name +
                    " or a derived type was expected.", ex);
                if (!allowDerivedTypes && ex.GetType () != typeof (TException))
                    throw new Exception ("Delegate threw exception of type " +
                    ex.GetType ().Name + ", but " + typeof (TException).Name +
                    " was expected.", ex);
                return (TException) ex;
            }
            throw new Exception ("Delegate did not throw expected exception " +
            typeof (TException).Name + ".");
        }
        public static Task<Exception> ThrowsAsync (Func<Task> action)
        {
            return ThrowsAsync<Exception> (action, true);
        }
    }
}
