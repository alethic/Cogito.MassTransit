using System.Collections.Generic;

using MassTransit;

namespace Cogito.MassTransit
{

    /// <summary>
    /// Produces the set of request messages dispatched by a multi-request from the current saga behavior context.
    /// </summary>
    public delegate IEnumerable<TRequest> EventMultiMessageFactory<TSaga, out TRequest>(BehaviorContext<TSaga> context)
        where TSaga : class, SagaStateMachineInstance;

    /// <summary>
    /// Produces the set of request messages dispatched by a multi-request from the current saga behavior context.
    /// </summary>
    public delegate IEnumerable<TRequest> EventMultiMessageFactory<TSaga, TMessage, out TRequest>(BehaviorContext<TSaga, TMessage> context)
        where TSaga : class, SagaStateMachineInstance
        where TMessage : class;

}
