using System;

using Automatonymous;
using Automatonymous.Events;

using Cogito.MassTransit.Automatonymous.Events;

using MassTransit;

namespace Cogito.MassTransit.Automatonymous
{

    /// <summary>
    /// Provides access to manage a multi-requests state.
    /// </summary>
    /// <typeparam name="TInstance"></typeparam>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    public interface IMultiRequestStateAccessor<TInstance, TState, TRequest, TResponse>
        where TInstance : class, SagaStateMachineInstance
        where TRequest : class
        where TResponse : class
    {

        /// <summary>
        /// Inserts a new request state item in the Pending status.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="requestId"></param>
        /// <returns></returns>
        TState Insert(InstanceContext<TInstance> context, Guid requestId);

        /// <summary>
        /// Adapts the request state item to a common interface.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        MultiRequestItemStatus GetStatus(InstanceContext<TInstance> context, TState state);

        /// <summary>
        /// Gets the response for the completed request state.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        TResponse GetResponse(InstanceContext<TInstance> context, TState state);

        /// <summary>
        /// Gets the fault for the faulted state item.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        Fault<TRequest> GetFault(InstanceContext<TInstance> context, TState state);

        /// <summary>
        /// Sets a request state item as completed.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="state"></param>
        /// <param name="response"></param>
        void SetCompleted(InstanceContext<TInstance> context, TState state, TResponse response);

        /// <summary>
        /// Sets a request state item as faulted.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="state"></param>
        /// <param name="fault"></param>
        void SetFaulted(InstanceContext<TInstance> context, TState state, Fault<TRequest> fault);

        /// <summary>
        /// Sets a request state item as timed out.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="state"></param>
        /// <param name="timeout"></param>
        void SetTimeoutExpired(InstanceContext<TInstance> context, TState state, RequestTimeoutExpired<TRequest> timeout);

    }

}
