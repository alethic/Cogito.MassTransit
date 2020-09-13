using System;
using System.Threading.Tasks;

using Automatonymous;

using GreenPipes;

using MassTransit;
using MassTransit.Context;

namespace Cogito.MassTransit.Automatonymous.Activities
{

    class RespondToActivity<TInstance, TData, TRequest, TResponse> : Activity<TInstance, TData>
        where TInstance : class, SagaStateMachineInstance
        where TData : class
        where TRequest : class
        where TResponse : class
    {

        readonly AsyncRequestTokenFactory<TInstance, TData, TRequest> requestTokenFactory;
        readonly AsyncEventMessageFactory<TInstance, TData, TResponse> messageFactory;
        readonly Action<SendContext<TResponse>> contextCallback;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="requestTokenFactory"></param>
        /// <param name="messageFactory"></param>
        /// <param name="contextCallback"></param>
        public RespondToActivity(AsyncRequestTokenFactory<TInstance, TData, TRequest> requestTokenFactory, AsyncEventMessageFactory<TInstance, TData, TResponse> messageFactory, Action<SendContext<TResponse>> contextCallback)
        {
            this.requestTokenFactory = requestTokenFactory ?? throw new ArgumentNullException(nameof(requestTokenFactory));
            this.messageFactory = messageFactory ?? throw new ArgumentNullException(nameof(messageFactory));
            this.contextCallback = contextCallback;
        }

        void Visitable.Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public void Probe(ProbeContext context)
        {
            context.CreateScope("respondTo");
        }

        public async Task Execute(BehaviorContext<TInstance, TData> context, Behavior<TInstance, TData> next)
        {
            var requestToken = await requestTokenFactory?.Invoke(context);

            var message = await messageFactory?.Invoke(context);

            var sendEndpoint = await context.GetSendEndpoint(requestToken.ResponseAddress);

            await sendEndpoint.Send(message, ctx =>
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