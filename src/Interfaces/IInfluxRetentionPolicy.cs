using System;

namespace AdysTech.InfluxDB.Client.Net
{
     public interface IInfluxRetentionPolicy
    {
        /// <summary>
        /// Gets/Sets the DB Name that the retention policy will be attached to
        /// </summary>
         string DBName { get; set; }

        /// <summary>
        /// Name of the renteion policy
        /// </summary>
         string Name { get; set; }

        /// <summary>
        /// Duration of the retention period that the policy is defining. If Duration is lower or equal TimeSpan.Zero retention policy will be Infinite
        /// </summary>
        TimeSpan Duration { get; set; }

        /// <summary>
        /// If this policy instance is the default policy for the DB
        /// </summary>
         bool IsDefault { get; set; }

        /// <summary>
        /// Number of nodes that the write should be confirmed
        /// </summary>
         int ReplicaN { get; set; }

        /// <summary>
        /// Determines the time range covered by a shard group
        /// </summary>
        TimeSpan ShardDuration { get; set; }

        /// <summary>
        /// if this object is saved to Influx DB
        /// </summary>
        bool Saved { get; }
    }
}