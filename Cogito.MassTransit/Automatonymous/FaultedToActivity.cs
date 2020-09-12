using System;
using System.Threading.Tasks;

using Automatonymous;

using GreenPipes;

using MassTransit;
using MassTransit.Events;
using MassTransit.Metadata;

namespace Cogito.MassTransit.Automatonymous
{

    class FaultedToActivity<TInstance, TData, TRequest> : Activity<TInstance, TData>
        where TInstance : class, SagaStateMachineInstance
        where TData : class
    {

        readonly AsyncRequestTokenFactory<TInstance, TData, TRequest> requestTokenFactory;
        readonly AsyncExceptionFactory<TInstance, TData, TRequest> exceptionFactory;
        readonly Action<SendContext<FaultEvent<TRequest>>> contextCallback;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="requestTokenFactory"></param>
        /// <param name="exceptionFactory"></param>
        /// <param name="contextCallback"></param>
        public FaultedToActivity(AsyncRequestTokenFactory<TInstance, TData, TRequest> requestTokenFactory, AsyncExceptionFactory<TInstance, TData, TRequest> exceptionFactory, Action<SendContext<FaultEvent<TRequest>>> contextCallback)
        {
            this.requestTokenFactory = requestTokenFactory ?? throw new ArgumentNullException(nameof(requestTokenFactory));
            this.exceptionFactory = exceptionFactory ?? throw new ArgumentNullException(nameof(exceptionFactory));
            this.contextCallback = contextCallback;
        }

        void Visitable.Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public void Probe(ProbeContext context)
        {
            context.CreateScope("faultedRespondTo");
        }

        public async Task Execute(BehaviorContext<TInstance, TData> context, Behavior<TInstance, TData> next)
        {
            var consumeContext = context.CreateConsumeContext();

            var requestToken = await requestTokenFactory?.Invoke(consumeContext);

            var exception = await exceptionFactory?.Invoke(consumeContext);

            var sendEndpoint = await consumeContext.GetSendEndpoint(requestToken.FaultAddress ?? requestToken.ResponseAddress);

            var fault = new FaultEvent<TRequest>(requestToken.Request, requestToken.MessageId, HostMetadataCache.Host, new[] { exception }, new string[0]);

            await sendEndpoint.Send(fault, ctx =>
            {
                ctx.CorrelationId = requestToken.CorrelationId;
                ctx.ConversationId = requestToken.ConversationId;
                ctx.RequestId = requestToken.RequestId;
                contextCallback?.Invoke(ctx);
            });

            await next.Execute(context).ConfigureAwait(false);
        }

        public Task Faulted<TException>(BehaviorExceptionContext<TInstance, TData, TException> context, Behavior<TInstance, TData> next) where TException : Exception
        {
            return next.Faulted(context);
        }

    }

}