using System;
using System.Threading.Tasks;

using Automatonymous;
using Automatonymous.Binders;
using Automatonymous.Contexts;

using Cogito.MassTransit.Automatonymous.Activities;

using MassTransit;

namespace Cogito.MassTransit.Automatonymous
{

    public static class RespondToExtensions
    {

        /// <summary>
        /// Captures a <see cref="RequestToken{TRequest}"/> instance from the current context.
        /// </summary>
        /// <typeparam name="TInstance"></typeparam>
        /// <typeparam name="TRequest"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        public static RequestToken<TRequest> CaptureRequestToken<TInstance, TRequest>(this BehaviorContext<TInstance, TRequest> context)
        {
            return CaptureRequestToken<TInstance, TRequest, RequestToken<TRequest>>(context);
        }

        /// <summary>
        /// Captures a <see cref="RequestToken{TRequest}"/> instance from the current context.
        /// </summary>
        /// <typeparam name="TInstance"></typeparam>
        /// <typeparam name="TRequest"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        public static TRequestToken CaptureRequestToken<TInstance, TRequest, TRequestToken>(this BehaviorContext<TInstance, TRequest> context)
            where TRequestToken : IRequestTokenSetter<TRequest>, new()
        {
            var token = new TRequestToken();
            CaptureRequestToken(context, token);
            return token;
        }

        /// <summary>
        /// Captures a <see cref="RequestToken{TRequest}"/> instance from the current context.
        /// </summary>
        /// <typeparam name="TInstance"></typeparam>
        /// <typeparam name="TRequest"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        public static void CaptureRequestToken<TInstance, TRequest>(this BehaviorContext<TInstance, TRequest> context, IRequestTokenSetter<TRequest> token)
        {
            var consumeContext = context.CreateConsumeContext();

            token.Request = context.Data;
            token.MessageId = consumeContext.MessageId.Value;
            token.RequestId = consumeContext.RequestId.Value;
            token.ConversationId = consumeContext.ConversationId;
            token.CorrelationId = consumeContext.CorrelationId;
            token.FaultAddress = consumeContext.FaultAddress;
            token.ResponseAddress = consumeContext.ResponseAddress;
        }

        /// <summary>
        /// Captures the incoming request into a <see cref="RequestToken{TRequest}"/>.
        /// </summary>
        /// <typeparam name="TInstance"></typeparam>
        /// <typeparam name="TRequest"></typeparam>
        /// <param name="source"></param>
        /// <param name="captured"></param>
        /// <returns></returns>
        public static EventActivityBinder<TInstance, TRequest> CaptureRequest<TInstance, TRequest>(this EventActivityBinder<TInstance, TRequest> source, Action<BehaviorContext<TInstance, TRequest>, RequestToken<TRequest>> captured)
            where TInstance : class, SagaStateMachineInstance
            where TRequest : class
        {
            return source.Then(context => captured(context, context.CaptureRequestToken()));
        }

        /// <summary>
        /// Responds to the previosly captured request token.
        /// </summary>
        /// <typeparam name="TInstance"></typeparam>
        /// <typeparam name="TData"></typeparam>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="source"></param>
        /// <param name="requestTokenFactory"></param>
        /// <param name="messageFactory"></param>
        /// <param name="contextCallback"></param>
        /// <returns></returns>
        public static EventActivityBinder<TInstance, TData> RespondToAsync<TInstance, TData, TRequest, TResponse>(this EventActivityBinder<TInstance, TData> source, AsyncRequestTokenFactory<TInstance, TData, TRequest> requestTokenFactory, AsyncEventMessageFactory<TInstance, TData, TResponse> messageFactory, Action<SendContext<TResponse>> contextCallback = null)
            where TInstance : class, SagaStateMachineInstance
            where TData : class
            where TRequest : class
            where TResponse : class
        {
            return source.Add(new RespondToActivity<TInstance, TData, TRequest, TResponse>(requestTokenFactory, messageFactory, contextCallback));
        }

        /// <summary>
        /// Responds to the previosly captured request token.
        /// </summary>
        /// <typeparam name="TInstance"></typeparam>
        /// <typeparam name="TData"></typeparam>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="source"></param>
        /// <param name="requestTokenFactory"></param>
        /// <param name="message"></param>
        /// <param name="contextCallback"></param>
        /// <returns></returns>
        public static EventActivityBinder<TInstance, TData> RespondToAsync<TInstance, TData, TRequest, TResponse>(this EventActivityBinder<TInstance, TData> source, AsyncRequestTokenFactory<TInstance, TData, TRequest> requestTokenFactory, TResponse message, Action<SendContext<TResponse>> contextCallback = null)
            where TInstance : class, SagaStateMachineInstance
            where TData : class
            where TRequest : class
            where TResponse : class
        {
            return source.Add(new RespondToActivity<TInstance, TData, TRequest, TResponse>(requestTokenFactory, context => Task.FromResult(message), contextCallback));
        }

        /// <summary>
        /// Responds to the previosly captured request token.
        /// </summary>
        /// <typeparam name="TInstance"></typeparam>
        /// <typeparam name="TData"></typeparam>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="source"></param>
        /// <param name="requestTokenFactory"></param>
        /// <param name="messageFactory"></param>
        /// <param name="contextCallback"></param>
        /// <returns></returns>
        public static EventActivityBinder<TInstance, TData> RespondTo<TInstance, TData, TRequest, TResponse>(this EventActivityBinder<TInstance, TData> source, RequestTokenFactory<TInstance, TData, TRequest> requestTokenFactory, EventMessageFactory<TInstance, TData, TResponse> messageFactory, Action<SendContext<TResponse>> contextCallback = null)
            where TInstance : class, SagaStateMachineInstance
            where TData : class
            where TRequest : class
            where TResponse : class
        {
            return source.Add(new RespondToActivity<TInstance, TData, TRequest, TResponse>(context => Task.FromResult(requestTokenFactory(context)), context => Task.FromResult(messageFactory(context)), contextCallback));
        }

        /// <summary>
        /// Responds to the previosly captured request token.
        /// </summary>
        /// <typeparam name="TInstance"></typeparam>
        /// <typeparam name="TData"></typeparam>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="source"></param>
        /// <param name="requestTokenFactory"></param>
        /// <param name="message"></param>
        /// <param name="contextCallback"></param>
        /// <returns></returns>
        public static EventActivityBinder<TInstance, TData> RespondTo<TInstance, TData, TRequest, TResponse>(this EventActivityBinder<TInstance, TData> source, RequestTokenFactory<TInstance, TData, TRequest> requestTokenFactory, TResponse message, Action<SendContext<TResponse>> contextCallback = null)
            where TInstance : class, SagaStateMachineInstance
            where TData : class
            where TRequest : class
            where TResponse : class
        {
            return source.Add(new RespondToActivity<TInstance, TData, TRequest, TResponse>(context => Task.FromResult(requestTokenFactory(context)), context => Task.FromResult(message), contextCallback));
        }

    }

}
