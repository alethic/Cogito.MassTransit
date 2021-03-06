﻿using MassTransit;

namespace Cogito.MassTransit.Automatonymous.Events
{

    /// <summary>
    /// Describes a finished <see cref="MultiRequest{TInstance, TItem, TRequest, TResponse}"/>.
    /// </summary>
    /// <typeparam name="TInstance"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    /// <typeparam name="TRequest"></typeparam>
    public interface MultiRequestFinishedItem<TRequest, TResponse>
        where TRequest : class
        where TResponse : class
    {

        /// <summary>
        /// Gets the status of the item.
        /// </summary>
        MultiRequestItemStatus Status { get; }

        /// <summary>
        /// If the request completed, gets the response.
        /// </summary>
        TResponse Response { get; }

        /// <summary>
        /// If the request faulted, gets the fault.
        /// </summary>
        Fault<TRequest> Fault { get; }

    }

}
