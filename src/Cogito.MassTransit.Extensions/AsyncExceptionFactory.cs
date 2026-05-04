using System;
using System.Threading.Tasks;

using MassTransit;

namespace Cogito.MassTransit
{

    /// <summary>
    /// Asynchronously resolves the <see cref="Exception"/> to fault with for a captured request.
    /// </summary>
    public delegate Task<Exception> AsyncExceptionFactory<in TSaga, in TMessage, TRequest>(SagaConsumeContext<TSaga, TMessage> context)
        where TSaga : class, ISaga
        where TMessage : class;

}