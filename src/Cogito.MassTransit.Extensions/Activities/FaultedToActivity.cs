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
        readonly Action<SendContext<FaultEvent<TRequest>>> contextCallback;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="requestTokenFactory"></param>
        /// <param name="exceptionFactory"></param>
        /// <param name="contextCallback"></param>
        public FaultedToActivity(AsyncRequestTokenFactory<TSaga, TMessage, TRequest> requestTokenFactory, AsyncExceptionFactory<TSaga, TMessage, TRequest> exceptionFactory, Action<SendContext<FaultEvent<TRequest>>> contextCallback)
        {
            this.requestTokenFactory = requestTokenFactory ?? throw new ArgumentNullException(nameof(requestTokenFactory));
            this.exceptionFactory = exceptionFactory ?? throw new ArgumentNullException(nameof(exceptionFactory));
            this.contextCallback = contextCallback;
        }

        void IVisitable.Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public void Probe(ProbeContext context)
        {
            context.CreateScope("faultedRespondTo");
        }

        public async Task Execute(BehaviorContext<TSaga, TMessage> context, IBehavior<TSaga, TMessage> next)
        {
            var requestToken = await requestTokenFactory?.Invoke(context);

            var exception = await exceptionFactory?.Invoke(context);

            var sendEndpoint = await context.GetSendEndpoint(requestToken.FaultAddress ?? requestToken.ResponseAddress);

            var fault = new FaultEvent<TRequest>(requestToken.Request, requestToken.MessageId, HostMetadataCache.Host, exception, new string[0]);

            await sendEndpoint.Send(fault, ctx =>
            {
                ctx.CorrelationId = requestToken.CorrelationId;
                ctx.ConversationId = requestToken.ConversationId;
                ctx.RequestId = requestToken.RequestId;
                contextCallback?.Invoke(ctx);
            });

            await next.Execute(context).ConfigureAwait(false);
        }

        public Task Faulted<TException>(BehaviorExceptionContext<TSaga, TMessage, TException> context, IBehavior<TSaga, TMessage> next) where TException : Exception
        {
            return next.Faulted(context);
        }

    }

}