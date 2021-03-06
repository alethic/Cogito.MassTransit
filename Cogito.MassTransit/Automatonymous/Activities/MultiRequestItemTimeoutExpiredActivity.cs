﻿using System;
using System.Threading.Tasks;

using Automatonymous;
using Automatonymous.Events;

using GreenPipes;

namespace Cogito.MassTransit.Automatonymous.Activities
{

    /// <summary>
    /// Executed when an item from a multi-request is completed.
    /// </summary>
    /// <typeparam name="TInstance"></typeparam>
    /// <typeparam name="TState"></typeparam>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    public class MultiRequestItemTimeoutExpiredActivity<TInstance, TState, TRequest, TResponse> : Activity<TInstance, RequestTimeoutExpired<TRequest>>
        where TInstance : class, SagaStateMachineInstance
        where TRequest : class
        where TResponse : class
    {

        readonly MultiRequest<TInstance, TState, TRequest, TResponse> request;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="request"></param>
        public MultiRequestItemTimeoutExpiredActivity(MultiRequest<TInstance, TState, TRequest, TResponse> request)
        {
            this.request = request ?? throw new ArgumentNullException(nameof(request));
        }

        public void Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public void Probe(ProbeContext context)
        {
            context.CreateScope("multiRequestItemTimeoutExpiredActivity");
        }

        public Task Execute(BehaviorContext<TInstance, RequestTimeoutExpired<TRequest>> context, Behavior<TInstance, RequestTimeoutExpired<TRequest>> next)
        {
            request.Accessor.SetTimeoutExpired(context, request.GetItem(context, context.CreateConsumeContext().RequestId.Value), context.Data);
            return next.Execute(context);
        }

        public Task Faulted<TException>(BehaviorExceptionContext<TInstance, RequestTimeoutExpired<TRequest>, TException> context, Behavior<TInstance, RequestTimeoutExpired<TRequest>> next) where TException : Exception
        {
            return next.Faulted(context);
        }

    }

}
