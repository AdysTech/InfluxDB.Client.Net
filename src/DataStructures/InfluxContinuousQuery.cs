using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdysTech.InfluxDB.Client.Net
{
    public class InfluxContinuousQuery : IInfluxContinuousQuery
    {
        /// <summary>
        /// Gets/Sets the DB Name that the CQ will be attached to
        /// </summary>
        public string DBName { get; set; }

        /// <summary>
        /// Name of the CQ
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///  Time range over which InfluxDB runs the CQ - For clause
        /// </summary>
        public TimeSpan ResampleDuration { get; set; }

        /// <summary>
        ///  How often InfluxDB runs the CQ - Resample clause
        /// </summary>
        public TimeSpan ResampleFrequency { get; set; }

        /// <summary>
        ///  Time interval in which InfluxDB runs the CQ - Group By clause
        /// </summary>
        public TimeSpan GroupByInterval { get; set; }

        /// <summary>
        /// Continuous Query that will be run every time
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        /// CQ defines RESAMPLE clause
        /// </summary>
        public bool HasResampleClause { get { return (ResampleDuration != TimeSpan.MinValue || ResampleFrequency != TimeSpan.MinValue); } }

        public InfluxContinuousQuery()
        {
            ResampleDuration = TimeSpan.MinValue;
            ResampleFrequency = TimeSpan.MinValue;
            GroupByInterval = TimeSpan.MinValue;
        }

        /// <summary>
        /// if this object is saved to Influx DB
        /// </summary>
        public bool Saved { get; internal set; }

        public bool Deleted { get; internal set; }

        internal string GetCreateSyntax()
        {
            if (!String.IsNullOrWhiteSpace(DBName) && !String.IsNullOrWhiteSpace(Name) && !String.IsNullOrWhiteSpace(Query))
            {
                if (ResampleDuration == TimeSpan.MinValue && ResampleFrequency == TimeSpan.MinValue)
                    return $"CREATE CONTINUOUS QUERY \"{Name}\" ON \"{DBName}\" BEGIN {Query} END";
                else if (ResampleDuration != TimeSpan.MinValue && ResampleFrequency != TimeSpan.MinValue)
                    return $"CREATE CONTINUOUS QUERY \"{Name}\" ON \"{DBName}\" RESAMPLE EVERY {ResampleFrequency.TotalMinutes}m FOR {ResampleDuration.TotalMinutes}m BEGIN {Query} END";
                else if (ResampleFrequency != TimeSpan.MinValue)
                {
                    if (ResampleFrequency.TotalMinutes > 1)
                        return $"CREATE CONTINUOUS QUERY \"{Name}\" ON \"{DBName}\" RESAMPLE EVERY {(int)ResampleFrequency.TotalMinutes}m BEGIN {Query} END";
                    else
                        return $"CREATE CONTINUOUS QUERY \"{Name}\" ON \"{DBName}\" RESAMPLE EVERY {(int)ResampleFrequency.TotalSeconds}s BEGIN {Query} END";
                }
                else if (ResampleDuration != TimeSpan.MinValue)
                    return $"CREATE CONTINUOUS QUERY \"{Name}\" ON \"{DBName}\" RESAMPLE FOR {ResampleDuration.TotalMinutes}m BEGIN {Query} END";
            }
            else if (String.IsNullOrWhiteSpace(Query))
                throw new ArgumentException("Query part of CQ is not defined");
            else if (String.IsNullOrWhiteSpace(Name))
                throw new ArgumentException("CQ Name not set");
            else if (String.IsNullOrWhiteSpace(DBName))
                throw new ArgumentException("DBName for CQ is not set");
            return null;
        }

        internal string GetDropSyntax()
        {
            if (!String.IsNullOrWhiteSpace(DBName) && !String.IsNullOrWhiteSpace(Name))
            {
                return $"DROP CONTINUOUS QUERY \"{Name}\" ON \"{DBName}\"";
            }
            else if (String.IsNullOrWhiteSpace(Name))
                throw new ArgumentException("CQ Name not set");
            else if (String.IsNullOrWhiteSpace(DBName))
                throw new ArgumentException("DBName for CQ is not set");
            return null;
        }
    }
}
