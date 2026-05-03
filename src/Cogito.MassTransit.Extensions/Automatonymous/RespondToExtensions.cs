using System;
using System.Threading.Tasks;

using Cogito.MassTransit.Automatonymous.Activities;

using MassTransit;

namespace Cogito.MassTransit.Automatonymous
{

    public static class RespondToExtensions
    {

        /// <summary>
        /// Captures a <see cref="RequestToken{TRequest}"/> instance from the current context.
        /// </summary>
        /// <typeparam name="TSaga"></typeparam>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        public static RequestToken<TMessage> CaptureRequestToken<TSaga, TMessage>(this BehaviorContext<TSaga, TMessage> context)
            where TSaga : class, SagaStateMachineInstance
            where TMessage : class
        {
            return CaptureRequestToken<TSaga, TMessage, RequestToken<TMessage>>(context);
        }

        /// <summary>
        /// Captures a <see cref="RequestToken{TRequest}"/> instance from the current context.
        /// </summary>
        /// <typeparam name="TSaga"></typeparam>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        public static TToken CaptureRequestToken<TSaga, TMessage, TToken>(this BehaviorContext<TSaga, TMessage> context)
            where TSaga : class, SagaStateMachineInstance
            where TMessage : class
            where TToken : IRequestTokenSetter<TMessage>, new()
        {
            var token = new TToken();
            CaptureRequestToken(context, token);
            return token;
        }

        /// <summary>
        /// Captures a <see cref="RequestToken{TRequest}"/> instance from the current context.
        /// </summary>
        /// <typeparam name="TSaga"></typeparam>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        public static void CaptureRequestToken<TSaga, TMessage>(this BehaviorContext<TSaga, TMessage> context, IRequestTokenSetter<TMessage> token)
            where TSaga : class, SagaStateMachineInstance
            where TMessage : class
        {
            token.Request = context.Message;
            token.MessageId = context.MessageId.Value;
            token.RequestId = context.RequestId.Value;
            token.ConversationId = context.ConversationId;
            token.CorrelationId = context.CorrelationId;
            token.FaultAddress = context.FaultAddress;
            token.ResponseAddress = context.ResponseAddress;
        }

        /// <summary>
        /// Captures the incoming request into a <see cref="RequestToken{TRequest}"/>.
        /// </summary>
        /// <typeparam name="TSaga"></typeparam>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="source"></param>
        /// <param name="captured"></param>
        /// <returns></returns>
        public static EventActivityBinder<TSaga, TMessage> CaptureRequest<TSaga, TMessage>(this EventActivityBinder<TSaga, TMessage> source, Action<BehaviorContext<TSaga, TMessage>, RequestToken<TMessage>> captured)
            where TSaga : class, SagaStateMachineInstance
            where TMessage : class
        {
            return source.Then(context => captured(context, context.CaptureRequestToken()));
        }

        /// <summary>
        /// Responds to the previosly captured request token.
        /// </summary>
        /// <typeparam name="TSaga"></typeparam>
        /// <typeparam name="TMessage"></typeparam>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="source"></param>
        /// <param name="requestTokenFactory"></param>
        /// <param name="messageFactory"></param>
        /// <param name="contextCallback"></param>
        /// <returns></returns>
        public static EventActivityBinder<TSaga, TMessage> RespondToAsync<TSaga, TMessage, TRequest, TResponse>(this EventActivityBinder<TSaga, TMessage> source, AsyncRequestTokenFactory<TSaga, TMessage, TRequest> requestTokenFactory, AsyncEventMessageFactory<TSaga, TMessage, TResponse> messageFactory, Action<SendContext<TResponse>> contextCallback = null)
            where TSaga : class, SagaStateMachineInstance
            where TMessage : class
            where TRequest : class
            where TResponse : class
        {
            return source.Add(new RespondToActivity<TSaga, TMessage, TRequest, TResponse>(requestTokenFactory, messageFactory, contextCallback));
        }

        /// <summary>
        /// Responds to the previosly captured request token.
        /// </summary>
        /// <typeparam name="TSaga"></typeparam>
        /// <typeparam name="TMessage"></typeparam>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="source"></param>
        /// <param name="requestTokenFactory"></param>
        /// <param name="message"></param>
        /// <param name="contextCallback"></param>
        /// <returns></returns>
        public static EventActivityBinder<TSaga, TMessage> RespondToAsync<TSaga, TMessage, TRequest, TResponse>(this EventActivityBinder<TSaga, TMessage> source, AsyncRequestTokenFactory<TSaga, TMessage, TRequest> requestTokenFactory, TResponse message, Action<SendContext<TResponse>> contextCallback = null)
            where TSaga : class, SagaStateMachineInstance
            where TMessage : class
            where TRequest : class
            where TResponse : class
        {
            return source.Add(new RespondToActivity<TSaga, TMessage, TRequest, TResponse>(requestTokenFactory, context => Task.FromResult(message), contextCallback));
        }

        /// <summary>
        /// Responds to the previosly captured request token.
        /// </summary>
        /// <typeparam name="TSaga"></typeparam>
        /// <typeparam name="TMessage"></typeparam>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="source"></param>
        /// <param name="requestTokenFactory"></param>
        /// <param name="messageFactory"></param>
        /// <param name="contextCallback"></param>
        /// <returns></returns>
        public static EventActivityBinder<TSaga, TMessage> RespondTo<TSaga, TMessage, TRequest, TResponse>(this EventActivityBinder<TSaga, TMessage> source, RequestTokenFactory<TSaga, TMessage, TRequest> requestTokenFactory, EventMessageFactory<TSaga, TMessage, TResponse> messageFactory, Action<SendContext<TResponse>> contextCallback = null)
            where TSaga : class, SagaStateMachineInstance
            where TMessage : class
            where TRequest : class
            where TResponse : class
        {
            return source.Add(new RespondToActivity<TSaga, TMessage, TRequest, TResponse>(context => Task.FromResult(requestTokenFactory(context)), context => Task.FromResult(messageFactory(context)), contextCallback));
        }

        /// <summary>
        /// Responds to the previosly captured request token.
        /// </summary>
        /// <typeparam name="TSaga"></typeparam>
        /// <typeparam name="TMessage"></typeparam>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="source"></param>
        /// <param name="requestTokenFactory"></param>
        /// <param name="message"></param>
        /// <param name="contextCallback"></param>
        /// <returns></returns>
        public static EventActivityBinder<TSaga, TMessage> RespondTo<TSaga, TMessage, TRequest, TResponse>(this EventActivityBinder<TSaga, TMessage> source, RequestTokenFactory<TSaga, TMessage, TRequest> requestTokenFactory, TResponse message, Action<SendContext<TResponse>> contextCallback = null)
            where TSaga : class, SagaStateMachineInstance
            where TMessage : class
            where TRequest : class
            where TResponse : class
        {
            return source.Add(new RespondToActivity<TSaga, TMessage, TRequest, TResponse>(context => Task.FromResult(requestTokenFactory(context)), context => Task.FromResult(message), contextCallback));
        }

    }

}
