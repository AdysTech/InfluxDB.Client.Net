using System;

namespace AdysTech.InfluxDB.Client.Net
{
   [AttributeUsage(AttributeTargets.Property, Inherited = true)]
   public class InfluxDBTime : Attribute { }
}
