using System;

namespace Cogito.MassTransit.Scheduling
{

    /// <summary>
    /// Describes a schedule.
    /// </summary>
    public class Schedule
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public Schedule()
        {

        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="name"></param>
        public Schedule(string groupName, string name)
        {
            GroupName = groupName;
            Name = name;
        }

        /// <summary>
        /// Unique identifying name to categorize this schedule.
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// Unique identifying name of the schedule within the group.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Point at which the schedule will be enabled.
        /// </summary>
        public DateTimeOffset? From { get; set; }

        /// <summary>
        /// Point at which the schedule will be disabled.
        /// </summary>
        public DateTimeOffset? Thru { get; set; }

        /// <summary>
        /// Interval on which to fire the trigger.
        /// </summary>
        public TimeSpan? Interval { get; set; }

        /// <summary>
        /// Number of times this trigger will fire after the first.
        /// </summary>
        public int RepeatCount { get; set; }

        /// <summary>
        /// Defines how the schedule should act when a misfire is encountered.
        /// </summary>
        public ScheduleMisfirePolicy MisfirePolicy { get; set; } = ScheduleMisfirePolicy.Default;

        /// <summary>
        /// Sets the group name.
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public Schedule WithGroupName(string groupName)
        {
            GroupName = groupName;
            return this;
        }

        /// <summary>
        /// Sets the name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Schedule WithName(string name)
        {
            Name = name;
            return this;
        }

        /// <summary>
        /// Sets the from date of the schedule.
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        public Schedule WithFrom(DateTimeOffset from)
        {
            From = from;
            return this;
        }

        /// <summary>
        /// Sets the thru date of the schedule.
        /// </summary>
        /// <param name="thru"></param>
        /// <returns></returns>
        public Schedule WithThru(DateTimeOffset thru)
        {
            Thru = thru;
            return this;
        }

        /// <summary>
        /// Sets the interval at which the schedule will be triggered.
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        public Schedule WithInterval(TimeSpan interval)
        {
            Interval = interval;
            return this;
        }

        /// <summary>
        /// Sets the number of times the schedule will be repeated after the first.
        /// </summary>
        /// <param name="repeatCount"></param>
        /// <returns></returns>
        public Schedule WithRepeatCount(int repeatCount)
        {
            RepeatCount = repeatCount;
            return this;
        }

        /// <summary>
        /// Sets the schedule to repeat forever.
        /// </summary>
        /// <returns></returns>
        public Schedule WithRepeatForever()
        {
            RepeatCount = -1;
            return this;
        }

        /// <summary>
        /// Configures the misfire policy.
        /// </summary>
        /// <param name="policy"></param>
        /// <returns></returns>
        public Schedule WithMisfirePolicy(ScheduleMisfirePolicy policy)
        {
            MisfirePolicy = policy;
            return this;
        }

    }

}
