using System;

namespace Cogito.MassTransit
{

    /// <summary>
    /// Raised when a request has not received a response within the configured timeout.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    public interface RequestTimeoutExpired<TRequest>
        where TRequest : class
    {

        /// <summary>
        /// The time at which the timeout was raised.
        /// </summary>
        DateTime Timestamp { get; }

        /// <summary>
        /// The time at which the request was scheduled to expire.
        /// </summary>
        DateTime ExpirationTime { get; }

        /// <summary>
        /// The correlation identifier of the saga instance.
        /// </summary>
        Guid CorrelationId { get; }

        /// <summary>
        /// The identifier of the request that timed out.
        /// </summary>
        Guid RequestId { get; }

    }

}
