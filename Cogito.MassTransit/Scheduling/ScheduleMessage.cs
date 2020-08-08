using System;

namespace Cogito.MassTransit.Scheduling
{

    /// <summary>
    /// Request to schedule a message.
    /// </summary>
    public class ScheduleMessage<T> :
        ScheduleMessage
    {

        /// <summary>
        /// Message to be sent.
        /// </summary>
        public T Payload { get; set; }

    }

    /// <summary>
    /// Request to schedule a message.
    /// </summary>
    public class ScheduleMessage
    {

        /// <summary>
        /// Types of the message to be sent.
        /// </summary>
        public string[] PayloadType { get; set; }

        /// <summary>
        /// Destination to send the message.
        /// </summary>
        public Uri Destination { get; set; }

        /// <summary>
        /// Correlation ID of the message.
        /// </summary>
        public string CorrelationId { get; set; }

        /// <summary>
        /// Conversation ID of the message.
        /// </summary>
        public string ConversationId { get; set; }

        /// <summary>
        /// Schedule upon which to send the message.
        /// </summary>
        public Schedule Schedule { get; set; }

    }

}
