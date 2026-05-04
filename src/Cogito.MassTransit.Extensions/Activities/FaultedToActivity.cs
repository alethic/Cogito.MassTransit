using System;
using System.Threading.Tasks;

using MassTransit;
using MassTransit.Events;
using MassTransit.Metadata;

namespace Cogito.MassTransit.Extensions.Activities
{

    class FaultedToActivity<TSaga, TMessage, TRequest> : IStateMachineActivity<TSaga, TMessage>
        where TSaga : class, SagaStateMachineInstance
        where TMessage : class
    {

        readonly AsyncRequestTokenFactory<TSaga, TMessage, TRequest> requestTokenFactory;
        readonly AsyncExceptionFactory<TSaga, TMessage, TRequest> exceptionFactory;
        readonly Action<SendContext<FaultEvent<TRequest>>>? contextCallback;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="requestTokenFactory"></param>
        /// <param name="exceptionFactory"></param>
        /// <param name="contextCallback"></param>
        public FaultedToActivity(AsyncRequestTokenFactory<TSaga, TMessage, TRequest> requestTokenFactory, AsyncExceptionFactory<TSaga, TMessage, TRequest> exceptionFactory, Action<SendContext<FaultEvent<TRequest>>>? contextCallback)
        {
            this.requestTokenFactory = requestTokenFactory ?? throw new ArgumentNullException(nameof(requestTokenFactory));
            this.exceptionFactory = exceptionFactory ?? throw new ArgumentNullException(nameof(exceptionFactory));
            this.contextCallback = contextCallback;
        }

        void IVisitable.Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        /// <inheritdoc/>
        public void Probe(ProbeContext context)
        {
            context.CreateScope("faultedTo");
        }

        /// <inheritdoc/>
        public async Task Execute(BehaviorContext<TSaga, TMessage> context, IBehavior<TSaga, TMessage> next)
        {
            var requestToken = await requestTokenFactory.Invoke(context);
            if (requestToken is null)
            {
                LogContext.Debug?.Log(
                    "FaultedTo skipped on saga {SagaType} for request {RequestType}: request token factory returned null (no captured request available).",
                    typeof(TSaga).Name,
                    typeof(TRequest).Name);
            }
            else
            {
                var address = requestToken.FaultAddress ?? requestToken.ResponseAddress;
                if (address is null)
                {
                    LogContext.Debug?.Log(
                        "FaultedTo skipped on saga {SagaType} for request {RequestType}: captured request token has neither FaultAddress nor ResponseAddress (RequestId={RequestId}).",
                        typeof(TSaga).Name,
                        typeof(TRequest).Name,
                        requestToken.RequestId);
                }
                else
                {
                    var sendEndpoint = await context.GetSendEndpoint(address);
                    var exception = await exceptionFactory.Invoke(context);
                    var fault = new FaultEvent<TRequest>(requestToken.Request, requestToken.MessageId, HostMetadataCache.Host, exception, new string[0]);

                    await sendEndpoint.Send(fault, ctx =>
                    {
                        ctx.CorrelationId = requestToken.CorrelationId;
                        ctx.ConversationId = requestToken.ConversationId;
                        ctx.RequestId = requestToken.RequestId;
                        contextCallback?.Invoke(ctx);
                    });
                }
            }

            await next.Execute(context).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public Task Faulted<TException>(BehaviorExceptionContext<TSaga, TMessage, TException> context, IBehavior<TSaga, TMessage> next) where TException : Exception
        {
            return next.Faulted(context);
        }

    }

}