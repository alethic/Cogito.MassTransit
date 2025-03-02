using System;
using System.Threading.Tasks;

using Automatonymous;

using GreenPipes;

using MassTransit;

namespace Cogito.MassTransit.Automatonymous.Activities
{

    /// <summary>
    /// Executed when an item from a multi-request is completed.
    /// </summary>
    /// <typeparam name="TInstance"></typeparam>
    /// <typeparam name="TState"></typeparam>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    public class MultiRequestItemFaultedActivity<TInstance, TState, TRequest, TResponse> : Activity<TInstance, Fault<TRequest>>
        where TInstance : class, SagaStateMachineInstance
        where TRequest : class
        where TResponse : class
    {

        readonly MultiRequest<TInstance, TState, TRequest, TResponse> request;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="request"></param>
        public MultiRequestItemFaultedActivity(MultiRequest<TInstance, TState, TRequest, TResponse> request)
        {
            this.request = request ?? throw new ArgumentNullException(nameof(request));
        }

        public void Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public void Probe(ProbeContext context)
        {
            context.CreateScope("multiRequestItemFaultedActivity");
        }

        public Task Execute(BehaviorContext<TInstance, Fault<TRequest>> context, Behavior<TInstance, Fault<TRequest>> next)
        {
            request.Accessor.SetFaulted(context, request.GetItem(context, context.CreateConsumeContext().RequestId.Value), context.Data);
            return next.Execute(context);
        }

        public Task Faulted<TException>(BehaviorExceptionContext<TInstance, Fault<TRequest>, TException> context, Behavior<TInstance, Fault<TRequest>> next) where TException : Exception
        {
            return next.Faulted(context);
        }

    }

}
