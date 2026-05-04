using System;
using System.Threading.Tasks;

using Cogito.MassTransit.Extensions.Activities;

using MassTransit;

namespace Cogito.MassTransit
{

    /// <summary>
    /// Provides extension methods for responding to a previously captured request token from inside a saga state
    /// machine event activity.
    /// </summary>
    public static class RespondToExtensions
    {

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
        public static EventActivityBinder<TSaga, TMessage> RespondToAsync<TSaga, TMessage, TRequest, TResponse>(this EventActivityBinder<TSaga, TMessage> source, AsyncRequestTokenFactory<TSaga, TMessage, TRequest> requestTokenFactory, AsyncEventMessageFactory<TSaga, TMessage, TResponse> messageFactory, Action<SendContext<TResponse>>? contextCallback = null)
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
        public static EventActivityBinder<TSaga, TMessage> RespondToAsync<TSaga, TMessage, TRequest, TResponse>(this EventActivityBinder<TSaga, TMessage> source, AsyncRequestTokenFactory<TSaga, TMessage, TRequest> requestTokenFactory, TResponse message, Action<SendContext<TResponse>>? contextCallback = null)
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
        public static EventActivityBinder<TSaga, TMessage> RespondTo<TSaga, TMessage, TRequest, TResponse>(this EventActivityBinder<TSaga, TMessage> source, RequestTokenFactory<TSaga, TMessage, TRequest> requestTokenFactory, EventMessageFactory<TSaga, TMessage, TResponse> messageFactory, Action<SendContext<TResponse>>? contextCallback = null)
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
        public static EventActivityBinder<TSaga, TMessage> RespondTo<TSaga, TMessage, TRequest, TResponse>(this EventActivityBinder<TSaga, TMessage> source, RequestTokenFactory<TSaga, TMessage, TRequest> requestTokenFactory, TResponse message, Action<SendContext<TResponse>>? contextCallback = null)
            where TSaga : class, SagaStateMachineInstance
            where TMessage : class
            where TRequest : class
            where TResponse : class
        {
            return source.Add(new RespondToActivity<TSaga, TMessage, TRequest, TResponse>(context => Task.FromResult(requestTokenFactory(context)), context => Task.FromResult(message), contextCallback));
        }

    }

}
