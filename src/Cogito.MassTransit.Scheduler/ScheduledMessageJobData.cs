namespace Cogito.MassTransit.Scheduler
{

    class ScheduledMessageJobData
    {

        /// <summary>
        /// Types of the message.
        /// </summary>
        public string[] MessageType { get; set; }

        /// <summary>
        /// Body of the message.
        /// </summary>
        public string Message { get; set; }

    }

}
