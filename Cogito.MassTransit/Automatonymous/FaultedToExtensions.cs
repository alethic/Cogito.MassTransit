using System;
using System.Threading.Tasks;

using Automatonymous;
using Automatonymous.Binders;

using MassTransit;
using MassTransit.Events;

namespace Cogito.MassTransit.Automatonymous
{

    public static class FaultedToExtensions
    {

        /// <summary>
        /// Responds to a stored request with a fault.
        /// </summary>
        /// <typeparam name="TInstance"></typeparam>
        /// <typeparam name="TData"></typeparam>
        /// <typeparam name="TRequest"></typeparam>
        /// <param name="source"></param>
        /// <param name="requestTokenFactory"></param>
        /// <param name="exceptionFactory"></param>
        /// <param name="contextCallback"></param>
        /// <returns></returns>
        public static EventActivityBinder<TInstance, TData> FaultedToAsync<TInstance, TData, TRequest>(this EventActivityBinder<TInstance, TData> source, AsyncRequestTokenFactory<TInstance, TData, TRequest> requestTokenFactory, AsyncExceptionFactory<TInstance, TData, TRequest> exceptionFactory, Action<SendContext<FaultEvent<TRequest>>> contextCallback = null)
            where TInstance : class, SagaStateMachineInstance
            where TData : class
            where TRequest : class
        {
            return source.Add(new FaultedToActivity<TInstance, TData, TRequest>(requestTokenFactory, exceptionFactory, contextCallback));
        }

        /// <summary>
        /// Responds to a stored request with a fault.
        /// </summary>
        /// <typeparam name="TInstance"></typeparam>
        /// <typeparam name="TData"></typeparam>
        /// <typeparam name="TRequest"></typeparam>
        /// <param name="source"></param>
        /// <param name="requestTokenFactory"></param>
        /// <param name="exception"></param>
        /// <param name="contextCallback"></param>
        /// <returns></returns>
        public static EventActivityBinder<TInstance, TData> FaultedToAsync<TInstance, TData, TRequest>(this EventActivityBinder<TInstance, TData> source, AsyncRequestTokenFactory<TInstance, TData, TRequest> requestTokenFactory, Exception exception, Action<SendContext<FaultEvent<TRequest>>> contextCallback = null)
            where TInstance : class, SagaStateMachineInstance
            where TData : class
            where TRequest : class
        {
            return source.Add(new FaultedToActivity<TInstance, TData, TRequest>(requestTokenFactory, context => Task.FromResult(exception), contextCallback));
        }

        /// <summary>
        /// Responds to a stored request with a fault.
        /// </summary>
        /// <typeparam name="TInstance"></typeparam>
        /// <typeparam name="TData"></typeparam>
        /// <typeparam name="TRequest"></typeparam>
        /// <param name="source"></param>
        /// <param name="requestTokenFactory"></param>
        /// <param name="exceptionFactory"></param>
        /// <param name="contextCallback"></param>
        /// <returns></returns>
        public static EventActivityBinder<TInstance, TData> FaultedTo<TInstance, TData, TRequest>(this EventActivityBinder<TInstance, TData> source, RequestTokenFactory<TInstance, TData, TRequest> requestTokenFactory, ExceptionFactory<TInstance, TData, TRequest> exceptionFactory, Action<SendContext<FaultEvent<TRequest>>> contextCallback = null)
            where TInstance : class, SagaStateMachineInstance
            where TData : class
            where TRequest : class
        {
            return source.Add(new FaultedToActivity<TInstance, TData, TRequest>(context => Task.FromResult(requestTokenFactory(context)), context => Task.FromResult(exceptionFactory(context)), contextCallback));
        }

        /// <summary>
        /// Responds to a stored request with a fault.
        /// </summary>
        /// <typeparam name="TInstance"></typeparam>
        /// <typeparam name="TData"></typeparam>
        /// <typeparam name="TRequest"></typeparam>
        /// <param name="source"></param>
        /// <param name="requestTokenFactory"></param>
        /// <param name="exception"></param>
        /// <param name="contextCallback"></param>
        /// <returns></returns>
        public static EventActivityBinder<TInstance, TData> FaultedTo<TInstance, TData, TRequest>(this EventActivityBinder<TInstance, TData> source, RequestTokenFactory<TInstance, TData, TRequest> requestTokenFactory, Exception exception, Action<SendContext<FaultEvent<TRequest>>> contextCallback = null)
            where TInstance : class, SagaStateMachineInstance
            where TData : class
            where TRequest : class
        {
            return source.Add(new FaultedToActivity<TInstance, TData, TRequest>(context => Task.FromResult(requestTokenFactory(context)), context => Task.FromResult(exception), contextCallback));
        }

    }

}
