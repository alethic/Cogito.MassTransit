namespace Cogito.MassTransit.Automatonymous.Events
{

    /// <summary>
    /// Describes the various completion results of a multi-request item.
    /// </summary>
    public enum MultiRequestItemStatus
    {

        /// <summary>
        /// The item is currently pending.
        /// </summary>
        Pending,

        /// <summary>
        /// The item successfully completed.
        /// </summary>
        Completed,

        /// <summary>
        /// The item was faulted.
        /// </summary>
        Faulted,

        /// <summary>
        /// The item timed out.
        /// </summary>
        TimeoutExpired,

    }

}