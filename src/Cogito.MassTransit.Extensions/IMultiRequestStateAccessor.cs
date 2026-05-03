using System;
using System.Threading.Tasks;

using Cogito.MassTransit.Events;

using MassTransit;

namespace Cogito.MassTransit
{

    /// <summary>
    /// Provides access to manage a multi-requests state.
    /// </summary>
    /// <typeparam name="TSaga"></typeparam>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    public interface IMultiRequestStateAccessor<TSaga, TState, TRequest, TResponse>
        where TSaga : class, SagaStateMachineInstance
        where TRequest : class
        where TResponse : class
    {

        /// <summary>
        /// Inserts a new request state item in the Pending status.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="request"></param>
        /// <param name="requestId"></param>
        /// <returns></returns>
        TState Insert(SagaConsumeContext<TSaga> context, TRequest request, Guid requestId);

        /// <summary>
        /// Adapts the request state item to a common interface.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        MultiRequestItemStatus GetStatus(SagaConsumeContext<TSaga> context, TState state);

        /// <summary>
        /// Gets the response for the completed request state.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        TResponse GetResponse(SagaConsumeContext<TSaga> context, TState state);

        /// <summary>
        /// Gets the fault for the faulted state item.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        Fault<TRequest> GetFault(SagaConsumeContext<TSaga> context, TState state);

        /// <summary>
        /// Sets a request state item as completed.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="state"></param>
        /// <param name="response"></param>
        void SetCompleted(SagaConsumeContext<TSaga> context, TState state, TResponse response);

        /// <summary>
        /// Sets a request state item as faulted.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="state"></param>
        /// <param name="fault"></param>
        void SetFaulted(SagaConsumeContext<TSaga> context, TState state, Fault<TRequest> fault);

        /// <summary>
        /// Sets a request state item as timed out.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="state"></param>
        /// <param name="timeout"></param>
        void SetTimeoutExpired(SagaConsumeContext<TSaga> context, TState state, RequestTimeoutExpired<TRequest> timeout);

        /// <summary>
        /// Clears the request state item collection.
        /// </summary>
        /// <param name="context"></param>
        Task Clear(SagaConsumeContext<TSaga> context);

    }

}
