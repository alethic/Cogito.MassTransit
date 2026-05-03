using System;
using System.Collections.Generic;
using System.Linq;

using MassTransit;

namespace Cogito.MassTransit.Events
{

    /// <summary>
    /// Event which is raised when all items have been finished.
    /// </summary>
    public class MultiRequestFinishedEvent<TSaga, TState, TRequest, TResponse> : MultiRequestFinished<TRequest, TResponse>
        where TSaga : class, SagaStateMachineInstance
        where TRequest : class
        where TResponse : class
    {

        /// <summary>
        /// Initializes a new <see cref="MultiRequestFinishedEvent{TSaga, TState, TRequest, TResponse}"/> from the given context and request.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static MultiRequestFinishedEvent<TSaga, TState, TRequest, TResponse> Init(SagaConsumeContext<TSaga> context, MultiRequest<TSaga, TState, TRequest, TResponse> request)
        {
            return new MultiRequestFinishedEvent<TSaga, TState, TRequest, TResponse>(
                request.GetItems(context)
                    .Select(i => new { RequestId = request.GetRequestId(context, i), State = i })
                    .Where(i => i.RequestId != null)
                    .ToDictionary(i => (Guid)i.RequestId!, i => (MultiRequestFinishedItem<TRequest, TResponse>)new MultiRequestFinishedItem<TSaga, TState, TRequest, TResponse>(context, request.Accessor, i.State)));
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="items"></param>
        public MultiRequestFinishedEvent(Dictionary<Guid, MultiRequestFinishedItem<TRequest, TResponse>> items)
        {
            Items = items ?? throw new ArgumentNullException(nameof(items));
        }

        /// <summary>
        /// Gets all of the finished state items.
        /// </summary>
        /// <returns></returns>
        public IReadOnlyDictionary<Guid, MultiRequestFinishedItem<TRequest, TResponse>> Items { get; set; }

    }

}
