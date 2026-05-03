using System;
using System.Collections.Generic;

using Cogito.MassTransit.Events;

using MassTransit;

namespace Cogito.MassTransit
{

    /// <summary>
    /// Describes a multi-request on a state machine.
    /// </summary>
    /// <typeparam name="TSaga"></typeparam>
    /// <typeparam name="TState"></typeparam>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    public interface MultiRequest<TSaga, TState, TRequest, TResponse>
        where TSaga : class, SagaStateMachineInstance
        where TRequest : class
        where TResponse : class
    {

        /// <summary>
        /// The name of the request
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The settings that are used for the request, including the timeout
        /// </summary>
        MultiRequestSettings Settings { get; }

        /// <summary>
        /// The event that is dispatched back to the state machine to signal all requests complete.
        /// </summary>
        Event<MultiRequestFinishedSignal> FinishedSignal { get; set; }

        /// <summary>
        /// The event that is raised when all of the requests complete.
        /// </summary>
        Event<MultiRequestFinished<TRequest, TResponse>> Finished { get; set; }

        /// <summary>
        /// The event that is raised when a request completes and the response is received.
        /// </summary>
        Event<TResponse> Completed { get; set; }

        /// <summary>
        /// The event raised when a request faults.
        /// </summary>
        Event<Fault<TRequest>> Faulted { get; set; }

        /// <summary>
        /// The event raised when the request times out with no response received.
        /// </summary>
        Event<RequestTimeoutExpired<TRequest>> TimeoutExpired { get; set; }

        /// <summary>
        /// The state that is transitioned to once the requests are pending.
        /// </summary>
        State Pending { get; set; }

        /// <summary>
        /// Gets the adaptor used to manipulate the state items.
        /// </summary>
        IMultiRequestStateAccessor<TSaga, TState, TRequest, TResponse> Accessor { get; }

        /// <summary>
        /// Gets the request ID stored on the state object.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        Guid? GetRequestId(SagaConsumeContext<TSaga> context, TState state);

        /// <summary>
        /// Gets all of the request items.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="requestId"></param>
        /// <returns></returns>
        TState GetItem(SagaConsumeContext<TSaga> context, Guid requestId);

        /// <summary>
        /// Gets all of the request items.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        IEnumerable<TState> GetItems(SagaConsumeContext<TSaga> context);

        /// <summary>
        /// Returns <c>true</c> if all of the requests are finished.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        bool IsFinished(SagaConsumeContext<TSaga> context);

    }

}
