using Automatonymous;
using Automatonymous.Binders;
using Automatonymous.Events;

using Cogito.MassTransit.Automatonymous.Activities;

using MassTransit.Context;

namespace Cogito.MassTransit.Automatonymous
{

    public static class MultiRequestExtensions
    {

        /// <summary>
        /// Send requests to the configured service endpoint, and setup the state machine to accept the responses.
        /// </summary>
        /// <typeparam name="TInstance">The state instance type</typeparam>
        /// <typeparam name="TData">The event data type</typeparam>
        /// <typeparam name="TRequest">The request message type</typeparam>
        /// <typeparam name="TResponse">The response message type</typeparam>
        /// <param name="binder">The event binder</param>
        /// <param name="request">The configured request to use</param>
        /// <param name="messageFactory">The request message factory</param>
        /// <returns></returns>
        public static EventActivityBinder<TInstance, TData> MultiRequest<TInstance, TData, TKey, TRequest, TResponse>(
            this EventActivityBinder<TInstance, TData> binder,
            MultiRequest<TInstance, TKey, TRequest, TResponse> request,
            EventMultiMessageFactory<TInstance, TData, TRequest> messageFactory)
            where TInstance : class, SagaStateMachineInstance
            where TData : class
            where TRequest : class
            where TResponse : class
        {
            ScheduleTokenId.UseTokenId<RequestTimeoutExpired<TRequest>>(x => x.RequestId);
            var activity = new MultiRequestActivity<TInstance, TData, TKey, TRequest, TResponse>(request, messageFactory);
            return binder.Add(activity);
        }

    }

}
