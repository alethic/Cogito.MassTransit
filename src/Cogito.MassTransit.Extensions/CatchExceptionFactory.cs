using System;

using MassTransit;

namespace Cogito.MassTransit
{

    /// <summary>
    /// Resolves the <see cref="Exception"/> to fault with from inside a <c>Catch</c> handler.
    /// </summary>
    public delegate Exception CatchExceptionFactory<TSaga, TMessage, TException, TRequest>(BehaviorExceptionContext<TSaga, TMessage, TException> context)
        where TSaga : class, SagaStateMachineInstance
        where TMessage : class
        where TException : Exception;

}
