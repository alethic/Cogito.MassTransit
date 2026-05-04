using System.Collections.Generic;

using MassTransit;

namespace Cogito.MassTransit
{

    /// <summary>
    /// Asynchronously produces the set of request messages dispatched by a multi-request from the current saga
    /// behavior context.
    /// </summary>
    public delegate IAsyncEnumerable<TRequest> AsyncEventMultiMessageFactory<TSaga, TRequest>(BehaviorContext<TSaga> context)
        where TSaga : class, SagaStateMachineInstance;

    /// <summary>
    /// Asynchronously produces the set of request messages dispatched by a multi-request from the current saga
    /// behavior context.
    /// </summary>
    public delegate IAsyncEnumerable<TRequest> AsyncEventMultiMessageFactory<TSaga, TMessage, TRequest>(BehaviorContext<TSaga, TMessage> context)
        where TSaga : class, SagaStateMachineInstance
        where TMessage : class;

}
