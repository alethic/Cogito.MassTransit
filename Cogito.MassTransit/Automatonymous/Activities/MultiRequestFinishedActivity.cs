using System;
using System.Threading.Tasks;

using Automatonymous;

using Cogito.MassTransit.Automatonymous.Events;

using GreenPipes;

namespace Cogito.MassTransit.Automatonymous.Activities
{

    /// <summary>
    /// Executed when a multi-request is finished.
    /// </summary>
    /// <typeparam name="TInstance"></typeparam>
    /// <typeparam name="TState"></typeparam>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    public class MultiRequestFinishedActivity<TInstance, TState, TRequest, TResponse> : Activity<TInstance, MultiRequestFinishedSignal<TInstance, TRequest, TResponse>>
        where TInstance : class, SagaStateMachineInstance
        where TRequest : class
        where TResponse : class
    {

        readonly MultiRequest<TInstance, TState, TRequest, TResponse> request;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="request"></param>
        public MultiRequestFinishedActivity(MultiRequest<TInstance, TState, TRequest, TResponse> request)
        {
            this.request = request ?? throw new ArgumentNullException(nameof(request));
        }

        public void Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public async Task Execute(BehaviorContext<TInstance, MultiRequestFinishedSignal<TInstance, TRequest, TResponse>> context, Behavior<TInstance, MultiRequestFinishedSignal<TInstance, TRequest, TResponse>> next)
        {
            var evt = MultiRequestFinishedEvent<TInstance, TState, TRequest, TResponse>.Init(context, request);

            // settings indicate we should clear the state
            if (request.Settings.ClearOnFinish)
                await request.Accessor.Clear(context);

            await context.Raise(request.Finished, evt);
            await next.Execute(context);
        }

        public Task Faulted<TException>(BehaviorExceptionContext<TInstance, MultiRequestFinishedSignal<TInstance, TRequest, TResponse>, TException> context, Behavior<TInstance, MultiRequestFinishedSignal<TInstance, TRequest, TResponse>> next) where TException : Exception
        {
            return next.Faulted(context);
        }

        public void Probe(ProbeContext context)
        {
            context.CreateScope("multiRequestItemFinishedActivity");
        }

    }

}
