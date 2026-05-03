using Cogito.MassTransit.Extensions.Activities;

using MassTransit;
using MassTransit.Scheduling;

namespace Cogito.MassTransit
{

    public static class MultiRequestExtensions
    {

        /// <summary>
        /// Send requests to the configured service endpoint, and setup the state machine to accept the responses.
        /// </summary>
        /// <typeparam name="TSaga">The state instance type</typeparam>
        /// <typeparam name="TMessage">The event data type</typeparam>
        /// <typeparam name="TRequest">The request message type</typeparam>
        /// <typeparam name="TResponse">The response message type</typeparam>
        /// <param name="binder">The event binder</param>
        /// <param name="request">The configured request to use</param>
        /// <param name="messageFactory">The request message factory</param>
        /// <returns></returns>
        public static EventActivityBinder<TSaga, TMessage> MultiRequest<TSaga, TMessage, TState, TRequest, TResponse>(
            this EventActivityBinder<TSaga, TMessage> binder,
            MultiRequest<TSaga, TState, TRequest, TResponse> request,
            EventMultiMessageFactory<TSaga, TMessage, TRequest> messageFactory)
            where TSaga : class, SagaStateMachineInstance
            where TMessage : class
            where TRequest : class
            where TResponse : class
        {
            ScheduleTokenId.UseTokenId<RequestTimeoutExpired<TRequest>>(x => x.RequestId);
            var activity = new MultiRequestActivity<TSaga, TMessage, TState, TRequest, TResponse>(request, messageFactory);
            return binder.Add(activity);
        }

        /// <summary>
        /// Send requests to the configured service endpoint, and setup the state machine to accept the responses.
        /// </summary>
        /// <typeparam name="TSaga">The state instance type</typeparam>
        /// <typeparam name="TMessage">The event data type</typeparam>
        /// <typeparam name="TRequest">The request message type</typeparam>
        /// <typeparam name="TResponse">The response message type</typeparam>
        /// <param name="binder">The event binder</param>
        /// <param name="request">The configured request to use</param>
        /// <param name="messageFactory">The request message factory</param>
        /// <returns></returns>
        public static EventActivityBinder<TSaga, TMessage> MultiRequest<TSaga, TMessage, TState, TRequest, TResponse>(
            this EventActivityBinder<TSaga, TMessage> binder,
            MultiRequest<TSaga, TState, TRequest, TResponse> request,
            AsyncEventMultiMessageFactory<TSaga, TMessage, TRequest> messageFactory)
            where TSaga : class, SagaStateMachineInstance
            where TMessage : class
            where TRequest : class
            where TResponse : class
        {
            ScheduleTokenId.UseTokenId<RequestTimeoutExpired<TRequest>>(x => x.RequestId);
            var activity = new MultiRequestActivity<TSaga, TMessage, TState, TRequest, TResponse>(request, messageFactory);
            return binder.Add(activity);
        }

    }

}
