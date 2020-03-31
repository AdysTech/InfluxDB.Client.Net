using System;

namespace AdysTech.InfluxDB.Client.Net
{
    public class ServiceUnavailableException:InvalidOperationException
    {
        public ServiceUnavailableException():base("InfluxDB service is not available")
        {
            
        }
        public ServiceUnavailableException(string Message) : base(Message)
        {

        }
    }
}
