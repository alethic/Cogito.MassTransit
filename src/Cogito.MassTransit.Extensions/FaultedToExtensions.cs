using System;
using System.Threading.Tasks;

using Cogito.MassTransit.Extensions.Activities;

using MassTransit;
using MassTransit.Events;

namespace Cogito.MassTransit
{

    /// <summary>
    /// Provides extension methods for sending a fault back to a previously captured request token from inside a saga
    /// state machine event activity or <c>Catch</c> handler.
    /// </summary>
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

        /// <summary>
        /// Responds to a stored request with a fault from inside a <c>Catch</c> handler.
        /// </summary>
        /// <typeparam name="TSaga"></typeparam>
        /// <typeparam name="TMessage"></typeparam>
        /// <typeparam name="TException"></typeparam>
        /// <typeparam name="TRequest"></typeparam>
        /// <param name="source"></param>
        /// <param name="requestTokenFactory"></param>
        /// <param name="exceptionFactory"></param>
        /// <param name="contextCallback"></param>
        /// <returns></returns>
        public static ExceptionActivityBinder<TSaga, TMessage, TException> FaultedToAsync<TSaga, TMessage, TException, TRequest>(this ExceptionActivityBinder<TSaga, TMessage, TException> source, AsyncRequestTokenFactory<TSaga, TMessage, TRequest> requestTokenFactory, AsyncCatchExceptionFactory<TSaga, TMessage, TException, TRequest> exceptionFactory, Action<SendContext<FaultEvent<TRequest>>>? contextCallback = null)
            where TSaga : class, SagaStateMachineInstance
            where TMessage : class
            where TException : Exception
            where TRequest : class
        {
            return source.Add(new FaultedToExceptionActivity<TSaga, TMessage, TException, TRequest>(requestTokenFactory, exceptionFactory, contextCallback));
        }

        /// <summary>
        /// Responds to a stored request with a fault from inside a <c>Catch</c> handler, using the caught exception.
        /// </summary>
        /// <typeparam name="TSaga"></typeparam>
        /// <typeparam name="TMessage"></typeparam>
        /// <typeparam name="TException"></typeparam>
        /// <typeparam name="TRequest"></typeparam>
        /// <param name="source"></param>
        /// <param name="requestTokenFactory"></param>
        /// <param name="contextCallback"></param>
        /// <returns></returns>
        public static ExceptionActivityBinder<TSaga, TMessage, TException> FaultedToAsync<TSaga, TMessage, TException, TRequest>(this ExceptionActivityBinder<TSaga, TMessage, TException> source, AsyncRequestTokenFactory<TSaga, TMessage, TRequest> requestTokenFactory, Action<SendContext<FaultEvent<TRequest>>>? contextCallback = null)
            where TSaga : class, SagaStateMachineInstance
            where TMessage : class
            where TException : Exception
            where TRequest : class
        {
            return source.Add(new FaultedToExceptionActivity<TSaga, TMessage, TException, TRequest>(requestTokenFactory, context => Task.FromResult((Exception)context.Exception), contextCallback));
        }

        /// <summary>
        /// Responds to a stored request with a fault from inside a <c>Catch</c> handler.
        /// </summary>
        /// <typeparam name="TSaga"></typeparam>
        /// <typeparam name="TMessage"></typeparam>
        /// <typeparam name="TException"></typeparam>
        /// <typeparam name="TRequest"></typeparam>
        /// <param name="source"></param>
        /// <param name="requestTokenFactory"></param>
        /// <param name="exceptionFactory"></param>
        /// <param name="contextCallback"></param>
        /// <returns></returns>
        public static ExceptionActivityBinder<TSaga, TMessage, TException> FaultedTo<TSaga, TMessage, TException, TRequest>(this ExceptionActivityBinder<TSaga, TMessage, TException> source, RequestTokenFactory<TSaga, TMessage, TRequest> requestTokenFactory, CatchExceptionFactory<TSaga, TMessage, TException, TRequest> exceptionFactory, Action<SendContext<FaultEvent<TRequest>>>? contextCallback = null)
            where TSaga : class, SagaStateMachineInstance
            where TMessage : class
            where TException : Exception
            where TRequest : class
        {
            return source.Add(new FaultedToExceptionActivity<TSaga, TMessage, TException, TRequest>(context => Task.FromResult(requestTokenFactory(context)), context => Task.FromResult(exceptionFactory(context)), contextCallback));
        }

        /// <summary>
        /// Responds to a stored request with a fault from inside a <c>Catch</c> handler, using the caught exception.
        /// </summary>
        /// <typeparam name="TSaga"></typeparam>
        /// <typeparam name="TMessage"></typeparam>
        /// <typeparam name="TException"></typeparam>
        /// <typeparam name="TRequest"></typeparam>
        /// <param name="source"></param>
        /// <param name="requestTokenFactory"></param>
        /// <param name="contextCallback"></param>
        /// <returns></returns>
        public static ExceptionActivityBinder<TSaga, TMessage, TException> FaultedTo<TSaga, TMessage, TException, TRequest>(this ExceptionActivityBinder<TSaga, TMessage, TException> source, RequestTokenFactory<TSaga, TMessage, TRequest> requestTokenFactory, Action<SendContext<FaultEvent<TRequest>>>? contextCallback = null)
            where TSaga : class, SagaStateMachineInstance
            where TMessage : class
            where TException : Exception
            where TRequest : class
        {
            return source.Add(new FaultedToExceptionActivity<TSaga, TMessage, TException, TRequest>(context => Task.FromResult(requestTokenFactory(context)), context => Task.FromResult((Exception)context.Exception), contextCallback));
        }

        /// <summary>
        /// Responds to a stored request with a fault from inside a <c>Catch</c> handler.
        /// </summary>
        /// <typeparam name="TSaga"></typeparam>
        /// <typeparam name="TMessage"></typeparam>
        /// <typeparam name="TException"></typeparam>
        /// <typeparam name="TRequest"></typeparam>
        /// <param name="source"></param>
        /// <param name="requestTokenFactory"></param>
        /// <param name="exception"></param>
        /// <param name="contextCallback"></param>
        /// <returns></returns>
        public static ExceptionActivityBinder<TSaga, TMessage, TException> FaultedTo<TSaga, TMessage, TException, TRequest>(this ExceptionActivityBinder<TSaga, TMessage, TException> source, RequestTokenFactory<TSaga, TMessage, TRequest> requestTokenFactory, Exception exception, Action<SendContext<FaultEvent<TRequest>>>? contextCallback = null)
            where TSaga : class, SagaStateMachineInstance
            where TMessage : class
            where TException : Exception
            where TRequest : class
        {
            return source.Add(new FaultedToExceptionActivity<TSaga, TMessage, TException, TRequest>(context => Task.FromResult(requestTokenFactory(context)), context => Task.FromResult(exception), contextCallback));
        }

    }

}
