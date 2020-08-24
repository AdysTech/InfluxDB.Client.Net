using System;

namespace AdysTech.InfluxDB.Client.Net
{
   [AttributeUsage(AttributeTargets.Property, Inherited = true)]
   public class InfluxDBTag : Attribute
   {
      public readonly string Name;
      public InfluxDBTag(string name)
      {
         Name = name;
      }
   }
}
