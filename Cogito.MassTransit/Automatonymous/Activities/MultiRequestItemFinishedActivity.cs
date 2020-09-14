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
    public class MultiRequestItemFinishedActivity<TInstance, TState, TRequest, TResponse> : Activity<TInstance>
        where TInstance : class, SagaStateMachineInstance
        where TRequest : class
        where TResponse : class
    {

        readonly MultiRequest<TInstance, TState, TRequest, TResponse> request;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="request"></param>
        public MultiRequestItemFinishedActivity(MultiRequest<TInstance, TState, TRequest, TResponse> request)
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
                await request.OnFinished(context);

            await next.Execute(context);
        }

        public async Task Execute<T>(BehaviorContext<TInstance, T> context, Behavior<TInstance, T> next)
        {
            if (request.IsFinished(context))
                await request.OnFinished(context);

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

    }

}
