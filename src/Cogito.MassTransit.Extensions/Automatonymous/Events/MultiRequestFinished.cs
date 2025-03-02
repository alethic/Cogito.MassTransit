using System;
using System.Collections.Generic;

namespace Cogito.MassTransit.Automatonymous.Events
{

    /// <summary>
    /// Describes a finished <see cref="MultiRequest{TInstance, TState, TRequest, TResponse}"/>.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    public interface MultiRequestFinished<TRequest, TResponse>
        where TRequest : class
        where TResponse : class
    {

        /// <summary>
        /// Gets the set of request items.
        /// </summary>
        IReadOnlyDictionary<Guid, MultiRequestFinishedItem<TRequest, TResponse>> Items { get; set; }

    }

}
