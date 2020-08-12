using System;

namespace AdysTech.InfluxDB.Client.Net
{
   [AttributeUsage(AttributeTargets.Property, Inherited = true)]
   public class InfluxDbTag : Attribute
   {
      public readonly string Name;
      public InfluxDbTag(string name)
      {
         Name = name;
      }
   }
}
