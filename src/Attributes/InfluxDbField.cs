using System;

namespace AdysTech.InfluxDB.Client.Net
{
   [AttributeUsage(AttributeTargets.Property, Inherited = true)]
   public class InfluxDbField : Attribute
   {
      public readonly string Name;
      public InfluxDbField(string name)
      {
         Name = name;
      }
   }
}
