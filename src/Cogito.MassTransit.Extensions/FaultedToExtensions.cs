using System;
using System.Threading.Tasks;

using Cogito.MassTransit.Extensions.Activities;

using MassTransit;
using MassTransit.Events;

namespace Cogito.MassTransit
{

    public static class FaultedToExtensions
    {

        /// <summary>
        /// Responds to a stored request with a fault.
        /// </summary>
        /// <typeparam name="TSaga"></typeparam>
        /// <typeparam name="TMessage"></typeparam>
        /// <typeparam name="TRequest"></typeparam>
        /// <param name="source"></param>
        /// <param name="requestTokenFactory"></param>
        /// <param name="exceptionFactory"></param>
        /// <param name="contextCallback"></param>
        /// <returns></returns>
        public static EventActivityBinder<TSaga, TMessage> FaultedToAsync<TSaga, TMessage, TRequest>(this EventActivityBinder<TSaga, TMessage> source, AsyncRequestTokenFactory<TSaga, TMessage, TRequest> requestTokenFactory, AsyncExceptionFactory<TSaga, TMessage, TRequest> exceptionFactory, Action<SendContext<FaultEvent<TRequest>>>? contextCallback = null)
            where TSaga : class, SagaStateMachineInstance
            where TMessage : class
            where TRequest : class
        {
            return source.Add(new FaultedToActivity<TSaga, TMessage, TRequest>(requestTokenFactory, exceptionFactory, contextCallback));
        }

        /// <summary>
        /// Responds to a stored request with a fault.
        /// </summary>
        /// <typeparam name="TSaga"></typeparam>
        /// <typeparam name="TMessage"></typeparam>
        /// <typeparam name="TRequest"></typeparam>
        /// <param name="source"></param>
        /// <param name="requestTokenFactory"></param>
        /// <param name="exception"></param>
        /// <param name="contextCallback"></param>
        /// <returns></returns>
        public static EventActivityBinder<TSaga, TMessage> FaultedToAsync<TSaga, TMessage, TRequest>(this EventActivityBinder<TSaga, TMessage> source, AsyncRequestTokenFactory<TSaga, TMessage, TRequest> requestTokenFactory, Exception exception, Action<SendContext<FaultEvent<TRequest>>>? contextCallback = null)
            where TSaga : class, SagaStateMachineInstance
            where TMessage : class
            where TRequest : class
        {
            return source.Add(new FaultedToActivity<TSaga, TMessage, TRequest>(requestTokenFactory, context => Task.FromResult(exception), contextCallback));
        }

        /// <summary>
        /// Responds to a stored request with a fault.
        /// </summary>
        /// <typeparam name="TSaga"></typeparam>
        /// <typeparam name="TMessage"></typeparam>
        /// <typeparam name="TRequest"></typeparam>
        /// <param name="source"></param>
        /// <param name="requestTokenFactory"></param>
        /// <param name="exceptionFactory"></param>
        /// <param name="contextCallback"></param>
        /// <returns></returns>
        public static EventActivityBinder<TSaga, TMessage> FaultedTo<TSaga, TMessage, TRequest>(this EventActivityBinder<TSaga, TMessage> source, RequestTokenFactory<TSaga, TMessage, TRequest> requestTokenFactory, ExceptionFactory<TSaga, TMessage, TRequest> exceptionFactory, Action<SendContext<FaultEvent<TRequest>>>? contextCallback = null)
            where TSaga : class, SagaStateMachineInstance
            where TMessage : class
            where TRequest : class
        {
            return source.Add(new FaultedToActivity<TSaga, TMessage, TRequest>(context => Task.FromResult(requestTokenFactory(context)), context => Task.FromResult(exceptionFactory(context)), contextCallback));
        }

        /// <summary>
        /// Responds to a stored request with a fault.
        /// </summary>
        /// <typeparam name="TSaga"></typeparam>
        /// <typeparam name="TMessage"></typeparam>
        /// <typeparam name="TRequest"></typeparam>
        /// <param name="source"></param>
        /// <param name="requestTokenFactory"></param>
        /// <param name="exception"></param>
        /// <param name="contextCallback"></param>
        /// <returns></returns>
        public static EventActivityBinder<TSaga, TMessage> FaultedTo<TSaga, TMessage, TRequest>(this EventActivityBinder<TSaga, TMessage> source, RequestTokenFactory<TSaga, TMessage, TRequest> requestTokenFactory, Exception exception, Action<SendContext<FaultEvent<TRequest>>>? contextCallback = null)
            where TSaga : class, SagaStateMachineInstance
            where TMessage : class
            where TRequest : class
        {
            return source.Add(new FaultedToActivity<TSaga, TMessage, TRequest>(context => Task.FromResult(requestTokenFactory(context)), context => Task.FromResult(exception), contextCallback));
        }

    }

}
