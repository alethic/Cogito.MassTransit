namespace Cogito.MassTransit.Scheduling
{

    /// <summary>
    /// Defines how the schedule should handle misfires.
    /// </summary>
    public enum ScheduleMisfirePolicy
    {

        Default,

        /// <summary>
        /// Instructs the scheduler that upon a mis-fire situation, the message will be sent now.
        /// </summary>
        FireNow,

        /// <summary>
        /// Instructs the scheduler that the message will never be evaluated for a misfire situation, and that the
        /// scheduler will simply try to send the message as soon as it can, and then update act as if it had been
        /// sent at the proper time.
        /// </summary>
        Ignore,

        /// <summary>
        /// Instructs the scheduler that upon a mis-fire situation, the message wants to be re-scheduled to the next
        /// scheduled time after 'now' and with the repeat count left unchanged.
        /// </summary>
        NextWithExistingCount,

        /// <summary>
        /// Instructs the scheduler that upon a mis-fire situation, the message wants to be re-scheduled to the next
        /// scheduled time after 'now', and with the repeat count set to what it would be, if it had not missed any
        /// firings. Does nothing, misfired executions are discarded. Then the scheduler waits for next scheduled
        /// interval and goes back to schedule.
        /// </summary>
        NextWithRemainingCount,

        /// <summary>
        /// Instructs the scheduler that upon a mis-fire situation, the message wants to be re-scheduled to 'now'
        /// with the repeat count left as-is. This does obey the Thru-time however, so if 'now' is after the
        /// end-time the message will not be sent again.
        /// </summary>
        NowWithExistingRepeatCount,

        /// <summary>
        /// Instructs the scheduler that upon a mis-fire situation, the message wants to be re-scheduled to 'now'
        /// with the repeat count set to what it would be, if it had not missed any intervals. This does obey the
        /// schedule thru-time however, so if 'now' is after the thru-time the message will not be sent again.
        /// </summary>
        NowWithRemainingRepeatCount,

    }

}
