using System;
using System.Threading.Tasks;

using MassTransit;

namespace Cogito.MassTransit.Extensions.Activities
{

    class RespondToActivity<TSaga, TMessage, TRequest, TResponse> : IStateMachineActivity<TSaga, TMessage>
        where TSaga : class, SagaStateMachineInstance
        where TMessage : class
        where TRequest : class
        where TResponse : class
    {

        readonly AsyncRequestTokenFactory<TSaga, TMessage, TRequest> requestTokenFactory;
        readonly AsyncEventMessageFactory<TSaga, TMessage, TResponse> messageFactory;
        readonly Action<SendContext<TResponse>>? contextCallback;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="requestTokenFactory"></param>
        /// <param name="messageFactory"></param>
        /// <param name="contextCallback"></param>
        public RespondToActivity(AsyncRequestTokenFactory<TSaga, TMessage, TRequest> requestTokenFactory, AsyncEventMessageFactory<TSaga, TMessage, TResponse> messageFactory, Action<SendContext<TResponse>>? contextCallback)
        {
            this.requestTokenFactory = requestTokenFactory ?? throw new ArgumentNullException(nameof(requestTokenFactory));
            this.messageFactory = messageFactory ?? throw new ArgumentNullException(nameof(messageFactory));
            this.contextCallback = contextCallback;
        }

        void IVisitable.Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        /// <inheritdoc/>
        public void Probe(ProbeContext context)
        {
            context.CreateScope("respondTo");
        }

        /// <inheritdoc/>
        public async Task Execute(BehaviorContext<TSaga, TMessage> context, IBehavior<TSaga, TMessage> next)
        {
            var requestToken = await requestTokenFactory.Invoke(context);
            if (requestToken is null)
            {
                LogContext.Debug?.Log(
                    "RespondTo skipped on saga {SagaType} for response {ResponseType}: request token factory returned null (no captured request available).",
                    typeof(TSaga).Name,
                    typeof(TResponse).Name);
            }
            else if (requestToken.ResponseAddress is null)
            {
                LogContext.Debug?.Log(
                    "RespondTo skipped on saga {SagaType} for response {ResponseType}: captured request token has no ResponseAddress (RequestId={RequestId}).",
                    typeof(TSaga).Name,
                    typeof(TResponse).Name,
                    requestToken.RequestId);
            }
            else
            {
                var message = await messageFactory.Invoke(context);
                var sendEndpoint = await context.GetSendEndpoint(requestToken.ResponseAddress);

                await sendEndpoint.Send(message, ctx =>
                {
                    ctx.CorrelationId = requestToken.CorrelationId;
                    ctx.ConversationId = requestToken.ConversationId;
                    ctx.RequestId = requestToken.RequestId;
                    contextCallback?.Invoke(ctx);
                });
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