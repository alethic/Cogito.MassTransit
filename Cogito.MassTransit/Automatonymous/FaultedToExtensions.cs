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

        public static EventActivityBinder<TInstance, TData> FaultedToAsync<TInstance, TData, TRequest>(this EventActivityBinder<TInstance, TData> source, AsyncRequestTokenFactory<TInstance, TData, TRequest> requestTokenFactory, AsyncExceptionFactory<TInstance, TData, TRequest> exceptionFactory, Action<SendContext<FaultEvent<TRequest>>> contextCallback = null)
            where TInstance : class, SagaStateMachineInstance
            where TData : class
            where TRequest : class
        {
            return source.Add(new FaultedToActivity<TInstance, TData, TRequest>(requestTokenFactory, exceptionFactory, contextCallback));
        }

        public static EventActivityBinder<TInstance, TData> FaultedToAsync<TInstance, TData, TRequest>(this EventActivityBinder<TInstance, TData> source, AsyncRequestTokenFactory<TInstance, TData, TRequest> requestTokenFactory, Exception exception, Action<SendContext<FaultEvent<TRequest>>> contextCallback = null)
            where TInstance : class, SagaStateMachineInstance
            where TData : class
            where TRequest : class
        {
            return source.Add(new FaultedToActivity<TInstance, TData, TRequest>(requestTokenFactory, context => Task.FromResult(exception), contextCallback));
        }

        public static EventActivityBinder<TInstance, TData> FaultedTo<TInstance, TData, TRequest>(this EventActivityBinder<TInstance, TData> source, RequestTokenFactory<TInstance, TData, TRequest> requestTokenFactory, ExceptionFactory<TInstance, TData, TRequest> exceptionFactory, Action<SendContext<FaultEvent<TRequest>>> contextCallback = null)
            where TInstance : class, SagaStateMachineInstance
            where TData : class
            where TRequest : class
        {
            return source.Add(new FaultedToActivity<TInstance, TData, TRequest>(context => Task.FromResult(requestTokenFactory(context)), context => Task.FromResult(exceptionFactory(context)), contextCallback));
        }

        public static EventActivityBinder<TInstance, TData> FaultedTo<TInstance, TData, TRequest>(this EventActivityBinder<TInstance, TData> source, RequestTokenFactory<TInstance, TData, TRequest> requestTokenFactory, Exception exception, Action<SendContext<FaultEvent<TRequest>>> contextCallback = null)
            where TInstance : class, SagaStateMachineInstance
            where TData : class
            where TRequest : class
        {
            return source.Add(new FaultedToActivity<TInstance, TData, TRequest>(context => Task.FromResult(requestTokenFactory(context)), context => Task.FromResult(exception), contextCallback));
        }

    }

}
