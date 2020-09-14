
using System;
using System.Collections.Generic;

using Automatonymous;

namespace Cogito.MassTransit.Automatonymous.Events
{

    /// <summary>
    /// Describes a finished <see cref="MultiRequest{TInstance, TState, TRequest, TResponse}"/>.
    /// </summary>
    /// <typeparam name="TInstance"></typeparam>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    public interface IMultiRequestFinished<TInstance, TRequest, TResponse>
        where TInstance : class, SagaStateMachineInstance
        where TRequest : class
        where TResponse : class
    {

        /// <summary>
        /// Gets the set of request items.
        /// </summary>
        IReadOnlyDictionary<Guid, IMultiRequestItem<TInstance, TRequest, TResponse>> Items { get; }

    }

}
