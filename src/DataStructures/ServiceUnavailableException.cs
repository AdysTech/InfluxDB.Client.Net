using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdysTech.InfluxDB.Client.Net
{
    [Serializable]
    public class ServiceUnavailableException:InvalidOperationException
    {
        public ServiceUnavailableException():base("InfluxDB service is not available")
        {
            
        }
    }
}
