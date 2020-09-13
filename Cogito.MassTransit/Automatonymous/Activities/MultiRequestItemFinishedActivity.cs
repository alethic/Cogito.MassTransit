using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Automatonymous;
using Automatonymous.Activities;

using Cogito.MassTransit.Automatonymous.Events;

using GreenPipes;

using MassTransit;
using MassTransit.Context;

namespace Cogito.MassTransit.Automatonymous.Activities
{

    /// <summary>
    /// Executed when an item from a multi-request is completed.
    /// </summary>
    /// <typeparam name="TInstance"></typeparam>
    /// <typeparam name="TState"></typeparam>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    public class MultiRequestItemFinishedActivity<TInstance, TState, TRequest, TResponse> : Activity<TInstance>
        where TInstance : class, SagaStateMachineInstance
        where TRequest : class
        where TResponse : class
    {

        readonly IMultiRequest<TInstance, TState, TRequest, TResponse> request;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="request"></param>
        public MultiRequestItemFinishedActivity(IMultiRequest<TInstance, TState, TRequest, TResponse> request)
        {
            this.request = request ?? throw new ArgumentNullException(nameof(request));
        }

        public void Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public async Task Execute(BehaviorContext<TInstance> context, Behavior<TInstance> next)
        {
            if (request.IsFinished(context))
                await context.CreateConsumeContext().Raise(request.Finished, new MultiRequestFinishedEvent(context, request));

            await next.Execute(context);
        }

        public async Task Execute<T>(BehaviorContext<TInstance, T> context, Behavior<TInstance, T> next)
        {
            if (request.IsFinished(context))
                await context.CreateConsumeContext().Raise(request.Finished, new MultiRequestFinishedEvent(context, request));

            await next.Execute(context);
        }

        public Task Faulted<TException>(BehaviorExceptionContext<TInstance, TException> context, Behavior<TInstance> next) where TException : Exception
        {
            return next.Faulted(context);
        }

        public Task Faulted<T, TException>(BehaviorExceptionContext<TInstance, T, TException> context, Behavior<TInstance, T> next) where TException : Exception
        {
            return next.Faulted(context);
        }

        public void Probe(ProbeContext context)
        {
            context.CreateScope("multiRequestItemFinishedActivity");
        }

        /// <summary>
        /// Event which is raised when all items have been finished.
        /// </summary>
        class MultiRequestFinishedEvent : IMultiRequestFinished<TInstance, TRequest, TResponse>
        {

            readonly BehaviorContext<TInstance> context;
            readonly Dictionary<Guid, IMultiRequestItem<TInstance, TRequest, TResponse>> items;

            /// <summary>
            /// Initializes a new instance.
            /// </summary>
            /// <param name="context"></param>
            /// <param name="request"></param>
            public MultiRequestFinishedEvent(BehaviorContext<TInstance> context, IMultiRequest<TInstance, TState, TRequest, TResponse> request)
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

                readonly InstanceContext<TInstance> context;
                readonly IMultiRequestStateAccessor<TInstance, TState, TRequest, TResponse> accessor;
                readonly TState state;

                /// <summary>
                /// Initializes a new instance.
                /// </summary>
                /// <param name="context"></param>
                /// <param name="accessor"></param>
                /// <param name="state"></param>
                public MultiRequestFinishedItem(InstanceContext<TInstance> context, IMultiRequestStateAccessor<TInstance, TState, TRequest, TResponse> accessor, TState state)
                {
                    this.context = context ?? throw new ArgumentNullException(nameof(context));
                    this.accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
                    this.state = state;
                }

                /// <summary>
                /// Gets the status of the request.
                /// </summary>
                public MultiRequestItemStatus Status => accessor.GetStatus(context, state);

                /// <summary>
                /// Gets the response data of the request.
                /// </summary>
                public TResponse Response => accessor.GetResponse(context, state);

                /// <summary>
                /// Gets the fault that occurred during the request.
                /// </summary>
                public Fault<TRequest> Fault => accessor.GetFault(context, state);

            }

        }

    }

}
