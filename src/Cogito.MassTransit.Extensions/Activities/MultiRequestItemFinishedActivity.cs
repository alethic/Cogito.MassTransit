using System;
using System.Threading.Tasks;

using Cogito.MassTransit.Events;

using MassTransit;

namespace Cogito.MassTransit.Extensions.Activities
{

    /// <summary>
    /// Executed after each item event to check whether all items in the multi-request have completed,
    /// and if so, dispatches a <see cref="MultiRequestFinishedSignal"/> back to the state machine.
    /// </summary>
    /// <typeparam name="TSaga"></typeparam>
    /// <typeparam name="TState"></typeparam>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    class MultiRequestItemFinishedActivity<TSaga, TState, TRequest, TResponse> : IStateMachineActivity<TSaga>
        where TSaga : class, SagaStateMachineInstance
        where TRequest : class
        where TResponse : class
    {

        readonly MultiRequest<TSaga, TState, TRequest, TResponse> request;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="request"></param>
        public MultiRequestItemFinishedActivity(MultiRequest<TSaga, TState, TRequest, TResponse> request)
        {
            this.request = request ?? throw new ArgumentNullException(nameof(request));
        }

        void IVisitable.Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public void Probe(ProbeContext context)
        {
            context.CreateScope("multiRequestItemFinished");
        }

        async Task ExecuteCore(SagaConsumeContext<TSaga> context)
        {
            if (request.IsFinished(context))
            {
                // dispatch signal to ourselves
                var endpoint = await context.GetSendEndpoint(context.ReceiveContext.InputAddress).ConfigureAwait(false);
                await endpoint.Send(new MultiRequestFinishedSignal(), s => s.CorrelationId = context.Saga.CorrelationId, context.CancellationToken).ConfigureAwait(false);
            }
        }

        public async Task Execute(BehaviorContext<TSaga> context, IBehavior<TSaga> next)
        {
            await ExecuteCore(context).ConfigureAwait(false);
            await next.Execute(context).ConfigureAwait(false);
        }

        public async Task Execute<T>(BehaviorContext<TSaga, T> context, IBehavior<TSaga, T> next)
            where T : class
        {
            await ExecuteCore(context).ConfigureAwait(false);
            await next.Execute(context).ConfigureAwait(false);
        }

        public Task Faulted<TException>(BehaviorExceptionContext<TSaga, TException> context, IBehavior<TSaga> next)
            where TException : Exception
        {
            return next.Faulted(context);
        }

        public Task Faulted<T, TException>(BehaviorExceptionContext<TSaga, T, TException> context, IBehavior<TSaga, T> next)
            where T : class
            where TException : Exception
        {
            return next.Faulted(context);
        }

    }

}
