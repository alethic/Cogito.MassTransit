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
    public class MultiRequestClearItemActivity<TInstance, TState, TRequest, TResponse> : Activity<TInstance>
        where TInstance : class, SagaStateMachineInstance
        where TRequest : class
        where TResponse : class
    {

        readonly IMultiRequest<TInstance, TState, TRequest, TResponse> request;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="request"></param>
        public MultiRequestClearItemActivity(IMultiRequest<TInstance, TState, TRequest, TResponse> request)
        {
            this.request = request ?? throw new ArgumentNullException(nameof(request));
        }

        public void Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public void Probe(ProbeContext context)
        {
            context.CreateScope("multiRequestClearItemActivity");
        }

        public Task Execute(BehaviorContext<TInstance> context, Behavior<TInstance> next)
        {
            return next.Execute(context);
        }

        public Task Execute<T>(BehaviorContext<TInstance, T> context, Behavior<TInstance, T> next)
        {
            return next.Execute(context);
        }

        public Task Faulted<TException>(BehaviorExceptionContext<TInstance, TException> context, Behavior<TInstance> next) where TException : Exception
        {
            return next.Faulted(context);
        }

        public Task Faulted<T, TException>(BehaviorExceptionContext<TInstance, T, TException> context, Behavior<TInstance, T> next) where TException : Exception
        {
            return next.Faulted(context);
        }

    }

}
