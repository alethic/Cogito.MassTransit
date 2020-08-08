using System;

namespace Cogito.MassTransit.Scheduling.Periodic
{

    /// <summary>
    /// Invoked when a periodic heartbeat occurs.
    /// </summary>
    public abstract class P
    {

        /// <summary>
        /// Interval of the pulse.
        /// </summary>
        public TimeSpan Interval { get; set; }

        /// <summary>
        /// Time at which this pulse was fired.
        /// </summary>
        public DateTimeOffset? FireTimeUtc { get; set; }

        /// <summary>
        /// Time at which this pulse will next fire.
        /// </summary>
        public DateTimeOffset? NextFireTimeUtc { get; set; }

        /// <summary>
        /// Previous time at which this pulse fired.
        /// </summary>
        public DateTimeOffset? PreviousFireTimeUtc { get; set; }

        /// <summary>
        /// The scheduled time at which this pulse was fired for.
        /// </summary>
        public DateTimeOffset? ScheduledFireTimeUtc { get; set; }

    }

}
