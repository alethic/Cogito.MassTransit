using System;
using System.Collections.Generic;
using System.Linq;

using Automatonymous;

namespace Cogito.MassTransit.Automatonymous.Events
{

    /// <summary>
    /// Event which is raised when all items have been finished.
    /// </summary>
    public class MultiRequestFinishedEvent<TInstance, TState, TRequest, TResponse> : MultiRequestFinished<TInstance, TRequest, TResponse>
        where TInstance : class, SagaStateMachineInstance
        where TRequest : class
        where TResponse : class
    {

        /// <summary>
        /// Initializes a new <see cref="MultiRequestFinishedEvent{TInstance, TState, TRequest, TResponse}"/> from the given context and request.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static MultiRequestFinishedEvent<TInstance, TState, TRequest, TResponse> Init(InstanceContext<TInstance> context, MultiRequest<TInstance, TState, TRequest, TResponse> request)
        {
            return new MultiRequestFinishedEvent<TInstance, TState, TRequest, TResponse>(
                request.GetItems(context)
                    .Select(i => new { RequestId = request.GetRequestId(context, i), State = i })
                    .Where(i => i.RequestId != null)
                    .ToDictionary(i => i.RequestId.Value, i => (MultiRequestFinishedItem<TInstance, TRequest, TResponse>)new MultiRequestFinishedItem<TInstance, TState, TRequest, TResponse>(context, request.Accessor, i.State)));
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="items"></param>
        public MultiRequestFinishedEvent(Dictionary<Guid, MultiRequestFinishedItem<TInstance, TRequest, TResponse>> items)
        {
            Items = items ?? throw new ArgumentNullException(nameof(items));
        }

        /// <summary>
        /// Gets all of the finished state items.
        /// </summary>
        /// <returns></returns>
        public IReadOnlyDictionary<Guid, MultiRequestFinishedItem<TInstance, TRequest, TResponse>> Items { get; set; }

    }

}
