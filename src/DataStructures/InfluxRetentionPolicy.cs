using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdysTech.InfluxDB.Client.Net
{
    public class InfluxRetentionPolicy : IInfluxRetentionPolicy
    {
        /// <summary>
        /// Gets/Sets the DB Name that the retention policy will be attached to
        /// </summary>
        public string DBName { get; set; }

        /// <summary>
        /// Name of the renteion policy
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Duration of the retention period that the policy is defining
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// If this policy instance is the default policy for the DB
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// Number of nodes that the write should be confirmed
        /// </summary>
        public int ReplicaN { get; set; }

        /// <summary>
        /// Determines the time range covered by a shard group
        /// </summary>
        public TimeSpan ShardDuration { get; set; }

        /// <summary>
        /// if this object is saved to Influx DB
        /// </summary>
        public bool Saved { get; internal set; }

        public InfluxRetentionPolicy ()
        {
            ReplicaN = 1;
        }

        internal string GetCreateSyntax ()
        {
            if (!String.IsNullOrWhiteSpace (DBName) && !String.IsNullOrWhiteSpace (Name) && Duration >= TimeSpan.FromMinutes (60))
                return $"CREATE RETENTION POLICY {Name} ON \"{DBName}\" DURATION {Duration.TotalMinutes}m REPLICATION {ReplicaN} {(IsDefault ? " DEFAULT" : "")}";
            else if (Duration < TimeSpan.FromMinutes (60))
                throw new ArgumentException ("Minimum retention duration is 1 hour");
            else if (String.IsNullOrWhiteSpace (Name))
                throw new ArgumentException ("Name not set");
            else if (String.IsNullOrWhiteSpace (DBName))
                throw new ArgumentException ("DBName is not set");
            return null;
        }
    }
}
