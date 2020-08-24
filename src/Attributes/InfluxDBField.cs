using System;

namespace AdysTech.InfluxDB.Client.Net
{
   [AttributeUsage(AttributeTargets.Property, Inherited = true)]
   public class InfluxDBField : Attribute
   {
      public readonly string Name;
      public InfluxDBField(string name)
      {
         Name = name;
      }
   }
}
