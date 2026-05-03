using System;
using System.Threading.Tasks;

using MassTransit;

namespace Cogito.MassTransit.Extensions.Activities
{

    /// <summary>
    /// Cancels the previously scheduled timeout for a multi-request item once it has resolved
    /// (completed, faulted, or timed out).
    /// </summary>
    /// <typeparam name="TSaga"></typeparam>
    /// <typeparam name="TState"></typeparam>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    class MultiRequestCancelItemTimeoutActivity<TSaga, TState, TRequest, TResponse> : IStateMachineActivity<TSaga>
        where TSaga : class, SagaStateMachineInstance
        where TRequest : class
        where TResponse : class
    {

        readonly MultiRequest<TSaga, TState, TRequest, TResponse> request;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="request"></param>
        public MultiRequestCancelItemTimeoutActivity(MultiRequest<TSaga, TState, TRequest, TResponse> request)
        {
            this.request = request ?? throw new ArgumentNullException(nameof(request));
        }

        void IVisitable.Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public void Probe(ProbeContext context)
        {
            context.CreateScope("multiRequestCancelItemTimeout");
        }

        Task ExecuteCore(SagaConsumeContext<TSaga> context)
        {
            var requestId = context.RequestId;
            if (requestId.HasValue && request.Settings.Timeout > TimeSpan.Zero)
            {
                if (context.TryGetPayload<MessageSchedulerContext>(out var schedulerContext))
                    return schedulerContext.CancelScheduledSend(context.ReceiveContext.InputAddress, requestId.Value);

                throw new ConfigurationException("A scheduler was not available to cancel the scheduled request timeout");
            }

            return Task.CompletedTask;
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
