using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Cogito.MassTransit.Events;

using MassTransit;

namespace Cogito.MassTransit.Extensions.Internal
{

    /// <summary>
    /// Implementation of the <see cref="MultiRequest{TSaga, TKey, TRequest, TResponse}"/> for a state machine.
    /// </summary>
    /// <typeparam name="TSaga"></typeparam>
    /// <typeparam name="TState"></typeparam>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    partial class StateMachineMultiRequest<TSaga, TState, TRequest, TResponse> :
        MultiRequest<TSaga, TState, TRequest, TResponse>
        where TSaga : class, SagaStateMachineInstance
        where TRequest : class
        where TResponse : class
    {

        readonly string name;
        readonly Func<TSaga, Guid, bool> filterFunc;
        readonly Func<TSaga, IEnumerable<TState>> itemsFunc;
        readonly Func<TState, Guid?> requestIdFunc;
        readonly IMultiRequestStateAccessor<TSaga, TState, TRequest, TResponse> accessor;
        readonly MultiRequestSettings settings;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="filterExpression"></param>
        /// <param name="itemsExpression"></param>
        /// <param name="requestIdExpression"></param>
        /// <param name="accessor"></param>
        /// <param name="settings"></param>
        public StateMachineMultiRequest(
            string name,
            Expression<Func<TSaga, Guid, bool>> filterExpression,
            Expression<Func<TSaga, IEnumerable<TState>>> itemsExpression,
            Expression<Func<TState, Guid?>> requestIdExpression,
            IMultiRequestStateAccessor<TSaga, TState, TRequest, TResponse> accessor,
            MultiRequestSettings settings)
        {
            this.name = name ?? throw new ArgumentNullException(nameof(name));
            this.filterFunc = filterExpression?.Compile() ?? throw new ArgumentNullException(nameof(filterExpression));
            this.itemsFunc = itemsExpression?.Compile() ?? throw new ArgumentNullException(nameof(itemsExpression));
            this.requestIdFunc = requestIdExpression?.Compile() ?? throw new ArgumentNullException(nameof(requestIdExpression));
            this.accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
            this.settings = settings;
        }

        /// <summary>
        /// Gets the name of the MultiRequest.
        /// </summary>
        public string Name => name;

        /// <summary>
        /// Gets the request settings to be applied to outgoing requests.
        /// </summary>
        public MultiRequestSettings Settings => settings;

        /// <summary>
        /// Signal when all requests have finished. Do not place any triggers on this event.
        /// </summary>
        public Event<MultiRequestFinishedSignal> FinishedSignal { get; set; }

        /// <summary>
        /// Raised when all requests have finished.
        /// </summary>
        public Event<MultiRequestFinished<TRequest, TResponse>> Finished { get; set; }

        /// <summary>
        /// Raised when a single request has completed.
        /// </summary>
        public Event<TResponse> Completed { get; set; }

        /// <summary>
        /// Raised when a single request has faulted.
        /// </summary>
        public Event<Fault<TRequest>> Faulted { get; set; }

        /// <summary>
        /// Raised when a single event has timed out.
        /// </summary>
        public Event<RequestTimeoutExpired<TRequest>> TimeoutExpired { get; set; }

        /// <summary>
        /// State machine is waiting for all requests to complete.
        /// </summary>
        public State Pending { get; set; }

        /// <summary>
        /// Gets the state adaptor for the specified instance.
        /// </summary>
        /// <returns></returns>
        public IMultiRequestStateAccessor<TSaga, TState, TRequest, TResponse> Accessor => accessor;

        /// <summary>
        /// Gets the request ID for the given state object.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public Guid? GetRequestId(SagaConsumeContext<TSaga> context, TState state) => requestIdFunc(state);

        /// <summary>
        /// Returns <c>true</c> if all of the requests are finished.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool IsFinished(SagaConsumeContext<TSaga> context)
        {
            return GetItems(context).All(i => accessor.GetStatus(context, i) != MultiRequestItemStatus.Pending);
        }

        /// <summary>
        /// Gets the specified state item.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="requestId"></param>
        /// <returns></returns>
        public TState GetItem(SagaConsumeContext<TSaga> context, Guid requestId)
        {
            return GetItems(context).FirstOrDefault(i => requestIdFunc(i) == requestId);
        }

        /// <summary>
        /// Gets all of the state items.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public IEnumerable<TState> GetItems(SagaConsumeContext<TSaga> context)
        {
            return itemsFunc(context.Saga) ?? Enumerable.Empty<TState>();
        }

        /// <summary>
        /// Returns <c>true</c> if the event matches a known request ID.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool CompletedEventFilter(BehaviorContext<TSaga, TResponse> context)
        {
            if (context.RequestId is Guid requestId)
                return filterFunc(context.Saga, requestId);
            else
                return false;
        }

        /// <summary>
        /// Returns <c>true</c> if the event matches a known request ID.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool FaultedEventFilter(BehaviorContext<TSaga, Fault<TRequest>> context)
        {
            if (context.RequestId is Guid requestId)
                return filterFunc(context.Saga, requestId);
            else
                return false;
        }

        /// <summary>
        /// Returns <c>true</c> if the event matches a known request ID.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool RequestTimeoutExpiredEventFilter(BehaviorContext<TSaga, RequestTimeoutExpired<TRequest>> context)
        {
            if (context.RequestId is Guid requestId)
                return filterFunc(context.Saga, requestId);
            else
                return false;
        }

        /// <summary>
        /// Returns <c>true</c> if the finished event matches the state machine.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool FinishedSignalEventFilter(BehaviorContext<TSaga, MultiRequestFinishedSignal> context)
        {
            return true;
        }

    }

}
