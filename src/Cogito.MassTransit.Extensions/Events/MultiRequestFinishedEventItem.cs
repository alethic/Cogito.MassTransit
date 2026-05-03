using MassTransit;

namespace Cogito.MassTransit.Events
{

    /// <summary>
    /// Describes a finished item.
    /// </summary>
    public struct MultiRequestFinishedItem<TSaga, TState, TRequest, TResponse> : MultiRequestFinishedItem<TRequest, TResponse>
        where TSaga : class, SagaStateMachineInstance
        where TRequest : class
        where TResponse : class
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="accessor"></param>
        /// <param name="state"></param>
        public MultiRequestFinishedItem(SagaConsumeContext<TSaga> context, IMultiRequestStateAccessor<TSaga, TState, TRequest, TResponse> accessor, TState state)
        {
            Status = accessor.GetStatus(context, state);
            Response = accessor.GetResponse(context, state);
            Fault = accessor.GetFault(context, state);
        }

        /// <summary>
        /// Gets the status of the request.
        /// </summary>
        public MultiRequestItemStatus Status { get; }

        /// <summary>
        /// Gets the response data of the request.
        /// </summary>
        public TResponse Response { get; }

        /// <summary>
        /// Gets the fault that occurred during the request.
        /// </summary>
        public Fault<TRequest> Fault { get; }

    }

}
