using System;
using System.Threading.Tasks;

using Cogito.MassTransit.Events;

using MassTransit;

namespace Cogito.MassTransit.Extensions.Activities
{

    /// <summary>
    /// Executed when a multi-request is finished.
    /// </summary>
    /// <typeparam name="TSaga"></typeparam>
    /// <typeparam name="TState"></typeparam>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    class MultiRequestFinishedActivity<TSaga, TState, TRequest, TResponse> : IStateMachineActivity<TSaga, MultiRequestFinishedSignal>
        where TSaga : class, SagaStateMachineInstance
        where TRequest : class
        where TResponse : class
    {

        readonly MultiRequest<TSaga, TState, TRequest, TResponse> request;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="request"></param>
        public MultiRequestFinishedActivity(MultiRequest<TSaga, TState, TRequest, TResponse> request)
        {
            this.request = request ?? throw new ArgumentNullException(nameof(request));
        }

        void IVisitable.Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        /// <inheritdoc/>
        public void Probe(ProbeContext context)
        {
            context.CreateScope("multiRequestFinished");
        }

        /// <inheritdoc/>
        public async Task Execute(BehaviorContext<TSaga, MultiRequestFinishedSignal> context, IBehavior<TSaga, MultiRequestFinishedSignal> next)
        {
            var evt = MultiRequestFinishedEvent<TSaga, TState, TRequest, TResponse>.Init(context, request);

            // settings indicate we should clear the state
            if (request.Settings.ClearOnFinish)
                await request.Accessor.Clear(context).ConfigureAwait(false);

            await context.Raise(request.Finished, evt).ConfigureAwait(false);
            await next.Execute(context).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public Task Faulted<TException>(BehaviorExceptionContext<TSaga, MultiRequestFinishedSignal, TException> context, IBehavior<TSaga, MultiRequestFinishedSignal> next)
            where TException : Exception
        {
            return next.Faulted(context);
        }

    }

}
