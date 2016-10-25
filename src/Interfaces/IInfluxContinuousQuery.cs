using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdysTech.InfluxDB.Client.Net
{
    public interface IInfluxContinuousQuery
    {

        /// <summary>
        /// Gets/Sets the DB Name that the CQ will be attached to
        /// </summary>
        string DBName { get; set; }

        /// <summary>
        /// Name of the CQ
        /// </summary>
        string Name { get; set; }

        /// <summary>
        ///  Time range over which InfluxDB runs the CQ - For clause
        /// </summary>
        TimeSpan ResampleDuration { get; set; }

        /// <summary>
        ///  How often InfluxDB runs the CQ - Resample clause
        /// </summary>
        TimeSpan ResampleFrequency { get; set; }

        /// <summary>
        ///  Time interval in which InfluxDB runs the CQ - Group By clause
        /// </summary>
        TimeSpan GroupByInterval { get; set; }

        /// <summary>
        /// Continuous Query that will be run every time
        /// </summary>
        string Query { get; set; }

        /// <summary>
        /// CQ defines RESAMPLE clause
        /// </summary>
        bool HasResampleClause{ get; }

        /// <summary>
        /// if this object is saved to Influx DB
        /// </summary>
        bool Saved { get; }

        /// <summary>
        /// if this object has been already dropped from Influx DB
        /// </summary>
        bool Deleted { get; }

    }
}

