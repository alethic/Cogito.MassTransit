using System;

namespace Cogito.MassTransit.Automatonymous
{

    /// <summary>
    /// Encapsulates information about a request.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    public interface IRequestTokenSetter<TRequest>
    {

        /// <summary>
        /// Gets the request.
        /// </summary>
        TRequest Request { set; }

        /// <summary>
        /// Gets the message ID of the request.
        /// </summary>
        Guid MessageId { set; }

        /// <summary>
        /// Gets the request ID of the request.
        /// </summary>
        Guid RequestId { set; }

        /// <summary>
        /// Gets the correlation ID of the request.
        /// </summary>
        Guid? CorrelationId { set; }

        /// <summary>
        /// Gets the conversation ID of the request.
        /// </summary>
        Guid? ConversationId { set; }

        /// <summary>
        /// Gets the destination address of the request.
        /// </summary>
        Uri ResponseAddress { set; }

        /// <summary>
        /// Gets the fault address of the request. 
        /// </summary>
        Uri FaultAddress { set; }

    }

}