using System;

namespace Cogito.MassTransit.Automatonymous
{

    /// <summary>
    /// Encapsulates information about a request.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    public interface IRequestToken<TRequest>
    {

        /// <summary>
        /// Gets the  request.
        /// </summary>
        TRequest Request { get; }

        /// <summary>
        /// Gets the message ID of the request.
        /// </summary>
        Guid MessageId { get; }

        /// <summary>
        /// Gets the request ID of the request.
        /// </summary>
        Guid RequestId { get; }

        /// <summary>
        /// Gets the correlation ID of the request.
        /// </summary>
        Guid? CorrelationId { get; }

        /// <summary>
        /// Gets the conversation ID of the request.
        /// </summary>
        Guid? ConversationId { get; }

        /// <summary>
        /// Gets the destination address of the request.
        /// </summary>
        Uri ResponseAddress { get; }

        /// <summary>
        /// Gets the fault address of the request. 
        /// </summary>
        Uri FaultAddress { get; }
    }

}