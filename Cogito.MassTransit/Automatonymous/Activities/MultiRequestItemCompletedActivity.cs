using System;
using System.Threading.Tasks;

using Automatonymous;

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
    public class MultiRequestItemCompletedActivity<TInstance, TState, TRequest, TResponse> : Activity<TInstance, TResponse>
        where TInstance : class, SagaStateMachineInstance
        where TRequest : class
        where TResponse : class
    {

        readonly IMultiRequest<TInstance, TState, TRequest, TResponse> request;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="request"></param>
        public MultiRequestItemCompletedActivity(IMultiRequest<TInstance, TState, TRequest, TResponse> request)
        {
            this.request = request ?? throw new ArgumentNullException(nameof(request));
        }

        public void Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public void Probe(ProbeContext context)
        {
            context.CreateScope("multiRequestItemCompletedActivity");
        }

        public Task Execute(BehaviorContext<TInstance, TResponse> context, Behavior<TInstance, TResponse> next)
        {
            request.Accessor.SetCompleted(context, request.GetItem(context, context.CreateConsumeContext().RequestId.Value), context.Data);
            return next.Execute(context);
        }

        public Task Faulted<TException>(BehaviorExceptionContext<TInstance, TResponse, TException> context, Behavior<TInstance, TResponse> next) where TException : Exception
        {
            return next.Faulted(context);
        }

    }

}
