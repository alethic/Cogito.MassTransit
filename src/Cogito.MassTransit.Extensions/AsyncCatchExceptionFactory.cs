using System;
using System.Threading.Tasks;

using MassTransit;

namespace Cogito.MassTransit
{

    /// <summary>
    /// Asynchronously resolves the <see cref="Exception"/> to fault with from inside a <c>Catch</c> handler.
    /// </summary>
    public delegate Task<Exception> AsyncCatchExceptionFactory<TSaga, TMessage, TException, TRequest>(BehaviorExceptionContext<TSaga, TMessage, TException> context)
        where TSaga : class, SagaStateMachineInstance
        where TMessage : class
        where TException : Exception;

}
