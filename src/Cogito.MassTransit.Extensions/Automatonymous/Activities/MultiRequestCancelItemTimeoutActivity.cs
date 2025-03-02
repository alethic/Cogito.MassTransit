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
    public class MultiRequestCancelItemTimeoutActivity<TInstance, TState, TRequest, TResponse> : Activity<TInstance>
        where TInstance : class, SagaStateMachineInstance
        where TRequest : class
        where TResponse : class
    {

        readonly MultiRequest<TInstance, TState, TRequest, TResponse> request;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="request"></param>
        public MultiRequestCancelItemTimeoutActivity(MultiRequest<TInstance, TState, TRequest, TResponse> request)
        {
            this.request = request ?? throw new ArgumentNullException(nameof(request));
        }

        public void Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public void Probe(ProbeContext context)
        {
            context.CreateScope("multiRequestCancelItemTimeout");
        }

        public async Task Execute(BehaviorContext<TInstance> context, Behavior<TInstance> next)
        {
            await Execute(context).ConfigureAwait(false);
            await next.Execute(context).ConfigureAwait(false);
        }

        public async Task Execute<T>(BehaviorContext<TInstance, T> context, Behavior<TInstance, T> next)
        {
            await Execute(context).ConfigureAwait(false);
            await next.Execute(context).ConfigureAwait(false);
        }

        Task Execute(BehaviorContext<TInstance> context)
        {
            var consumeContext = context.CreateConsumeContext();

            var requestId = consumeContext.RequestId;
            if (requestId.HasValue && request.Settings.Timeout > TimeSpan.Zero)
            {
                if (consumeContext.TryGetPayload<MessageSchedulerContext>(out var schedulerContext))
                    return schedulerContext.CancelScheduledSend(consumeContext.ReceiveContext.InputAddress, requestId.Value);

                throw new ConfigurationException("A scheduler was not available to cancel the scheduled request timeout");
            }

            return Task.CompletedTask;
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
