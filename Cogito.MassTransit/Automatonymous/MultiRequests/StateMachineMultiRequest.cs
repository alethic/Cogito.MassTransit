﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Automatonymous;
using Automatonymous.Events;

using Cogito.MassTransit.Automatonymous.Events;

using MassTransit;

namespace Cogito.MassTransit.Automatonymous.MultiRequests
{

    /// <summary>
    /// Implementation of the <see cref="MultiRequest{TInstance, TKey, TRequest, TResponse}"/> for a state machine.
    /// </summary>
    /// <typeparam name="TInstance"></typeparam>
    /// <typeparam name="TState"></typeparam>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    class StateMachineMultiRequest<TInstance, TState, TRequest, TResponse> :
        MultiRequest<TInstance, TState, TRequest, TResponse>
        where TInstance : class, SagaStateMachineInstance
        where TRequest : class
        where TResponse : class
    {

        readonly string name;
        readonly Func<TInstance, Guid, bool> filterFunc;
        readonly Func<TInstance, IEnumerable<TState>> itemsFunc;
        readonly Func<TState, Guid?> requestIdFunc;
        readonly IMultiRequestStateAccessor<TInstance, TState, TRequest, TResponse> accessor;
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
            Expression<Func<TInstance, Guid, bool>> filterExpression,
            Expression<Func<TInstance, IEnumerable<TState>>> itemsExpression,
            Expression<Func<TState, Guid?>> requestIdExpression,
            IMultiRequestStateAccessor<TInstance, TState, TRequest, TResponse> accessor,
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
        /// Raised when all requests have finished.
        /// </summary>
        public Event<IMultiRequestFinished<TInstance, TRequest, TResponse>> Finished { get; set; }

        /// <summary>
        /// Invoked when all requests are finished.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task OnFinished(BehaviorContext<TInstance> context)
        {
            await context.Raise(Finished, new MultiRequestFinishedEvent(context, this));

            if (settings.ClearOnFinish)
                await accessor.Clear(context);
        }

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
        public IMultiRequestStateAccessor<TInstance, TState, TRequest, TResponse> Accessor => accessor;

        /// <summary>
        /// Gets the request ID for the given state object.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public Guid? GetRequestId(InstanceContext<TInstance> context, TState state) => requestIdFunc(state);

        /// <summary>
        /// Returns <c>true</c> if all of the 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool IsFinished(InstanceContext<TInstance> context)
        {
            return GetItems(context).All(i => accessor.GetStatus(context, i) != MultiRequestItemStatus.Pending);
        }

        /// <summary>
        /// Gets the specified state item.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="requestId"></param>
        /// <returns></returns>
        public TState GetItem(InstanceContext<TInstance> context, Guid requestId)
        {
            return GetItems(context).FirstOrDefault(i => requestIdFunc(i) == requestId);
        }

        /// <summary>
        /// Gets all of the state items.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public IEnumerable<TState> GetItems(InstanceContext<TInstance> context)
        {
            return itemsFunc(context.Instance) ?? Enumerable.Empty<TState>();
        }

        /// <summary>
        /// Returns <c>true</c> if the event matches a known request ID.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool CompletedEventFilter(EventContext<TInstance, TResponse> context)
        {
            if (context.TryGetPayload<ConsumeContext<TResponse>>(out var consumeContext) && consumeContext.RequestId is Guid requestId)
                return filterFunc(context.Instance, requestId);
            else
                return false;
        }

        /// <summary>
        /// Returns <c>true</c> if the event matches a known request ID.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool FaultedEventFilter(EventContext<TInstance, Fault<TRequest>> context)
        {
            if (context.TryGetPayload<ConsumeContext<Fault<TRequest>>>(out var consumeContext) && consumeContext.RequestId is Guid requestId)
                return filterFunc(context.Instance, requestId);
            else
                return false;
        }

        /// <summary>
        /// Returns <c>true</c> if the event matches a known request ID.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool RequestTimeoutExpiredEventFilter(EventContext<TInstance, RequestTimeoutExpired<TRequest>> context)
        {
            if (context.TryGetPayload<ConsumeContext<RequestTimeoutExpired<TRequest>>>(out var consumeContext) && consumeContext.RequestId is Guid requestId)
                return filterFunc(context.Instance, requestId);
            else
                return false;
        }

        /// <summary>
        /// Event which is raised when all items have been finished.
        /// </summary>
        class MultiRequestFinishedEvent : IMultiRequestFinished<TInstance, TRequest, TResponse>
        {

            readonly InstanceContext<TInstance> context;
            readonly Dictionary<Guid, IMultiRequestItem<TInstance, TRequest, TResponse>> items;

            /// <summary>
            /// Initializes a new instance.
            /// </summary>
            /// <param name="context"></param>
            /// <param name="request"></param>
            public MultiRequestFinishedEvent(InstanceContext<TInstance> context, MultiRequest<TInstance, TState, TRequest, TResponse> request)
            {
                this.context = context ?? throw new ArgumentNullException(nameof(context));

                // index all of the request information
                items = request.GetItems(context)
                    .Select(i => new { RequestId = request.GetRequestId(context, i), State = i })
                    .Where(i => i.RequestId != null)
                    .ToDictionary(i => i.RequestId.Value, i => (IMultiRequestItem<TInstance, TRequest, TResponse>)new MultiRequestFinishedItem(context, request.Accessor, i.State));
            }

            /// <summary>
            /// Gets all of the finished state items.
            /// </summary>
            /// <returns></returns>
            public IReadOnlyDictionary<Guid, IMultiRequestItem<TInstance, TRequest, TResponse>> Items => items;

            /// <summary>
            /// Describes a finished item.
            /// </summary>
            struct MultiRequestFinishedItem : IMultiRequestItem<TInstance, TRequest, TResponse>
            {

                /// <summary>
                /// Initializes a new instance.
                /// </summary>
                /// <param name="context"></param>
                /// <param name="accessor"></param>
                /// <param name="state"></param>
                public MultiRequestFinishedItem(InstanceContext<TInstance> context, IMultiRequestStateAccessor<TInstance, TState, TRequest, TResponse> accessor, TState state)
                {
                    Status = accessor.GetStatus(context, state);
                    Response = accessor.GetResponse(context, state);
                    Fault = accessor.GetFault(context, state);
                }

                /// <summary>
                /// Gets the status of the request.
                /// </summary>
                public MultiRequestItemStatus Status { get; }

                /// <summary>
                /// Gets the response data of the request.
                /// </summary>
                public TResponse Response { get; }

                /// <summary>
                /// Gets the fault that occurred during the request.
                /// </summary>
                public Fault<TRequest> Fault { get; }

            }

        }

    }

}
