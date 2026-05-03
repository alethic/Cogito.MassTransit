using System.Collections.Generic;

using MassTransit;

namespace Cogito.MassTransit
{

    public delegate IAsyncEnumerable<TRequest> AsyncEventMultiMessageFactory<TSaga, TRequest>(BehaviorContext<TSaga> context)
        where TSaga : class, SagaStateMachineInstance;

    public delegate IAsyncEnumerable<TRequest> AsyncEventMultiMessageFactory<TSaga, TMessage, TRequest>(BehaviorContext<TSaga, TMessage> context)
        where TSaga : class, SagaStateMachineInstance
        where TMessage : class;

}
