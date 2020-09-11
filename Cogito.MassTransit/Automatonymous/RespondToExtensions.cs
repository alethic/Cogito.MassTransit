using System;
using System.Threading.Tasks;

using Automatonymous;
using Automatonymous.Binders;

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

        public static EventActivityBinder<TInstance, TData> RespondToAsync<TInstance, TData, TRequest, TResponse>(this EventActivityBinder<TInstance, TData> source, AsyncRequestTokenFactory<TInstance, TData, TRequest> requestTokenFactory, AsyncEventMessageFactory<TInstance, TData, TResponse> messageFactory, Action<SendContext<TResponse>> contextCallback = null)
            where TInstance : class, SagaStateMachineInstance
            where TData : class
            where TRequest : class
            where TResponse : class
        {
            return source.Add(new RespondToActivity<TInstance, TData, TRequest, TResponse>(requestTokenFactory, messageFactory, contextCallback));
        }

        public static EventActivityBinder<TInstance, TData> RespondToAsync<TInstance, TData, TRequest, TResponse>(this EventActivityBinder<TInstance, TData> source, AsyncRequestTokenFactory<TInstance, TData, TRequest> requestTokenFactory, TResponse message, Action<SendContext<TResponse>> contextCallback = null)
            where TInstance : class, SagaStateMachineInstance
            where TData : class
            where TRequest : class
            where TResponse : class
        {
            return source.Add(new RespondToActivity<TInstance, TData, TRequest, TResponse>(requestTokenFactory, context => Task.FromResult(message), contextCallback));
        }

        public static EventActivityBinder<TInstance, TData> RespondTo<TInstance, TData, TRequest, TResponse>(this EventActivityBinder<TInstance, TData> source, RequestTokenFactory<TInstance, TData, TRequest> requestTokenFactory, EventMessageFactory<TInstance, TData, TResponse> messageFactory, Action<SendContext<TResponse>> contextCallback = null)
            where TInstance : class, SagaStateMachineInstance
            where TData : class
            where TRequest : class
            where TResponse : class
        {
            return source.Add(new RespondToActivity<TInstance, TData, TRequest, TResponse>(context => Task.FromResult(requestTokenFactory(context)), context => Task.FromResult(messageFactory(context)), contextCallback));
        }

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
