using System.Collections.Generic;

using MassTransit;

namespace Cogito.MassTransit
{

    public delegate IEnumerable<TRequest> EventMultiMessageFactory<TSaga, out TRequest>(BehaviorContext<TSaga> context)
        where TSaga : class, SagaStateMachineInstance;

    public delegate IEnumerable<TRequest> EventMultiMessageFactory<TSaga, TMessage, out TRequest>(BehaviorContext<TSaga, TMessage> context)
        where TSaga : class, SagaStateMachineInstance
        where TMessage : class;

}
