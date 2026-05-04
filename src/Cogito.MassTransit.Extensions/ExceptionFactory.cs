using System;

using MassTransit;

namespace Cogito.MassTransit
{

    /// <summary>
    /// Resolves the <see cref="Exception"/> to fault with for a captured request.
    /// </summary>
    public delegate Exception ExceptionFactory<in TSaga, in TMessage, TRequest>(SagaConsumeContext<TSaga, TMessage> context)
        where TSaga : class, ISaga
        where TMessage : class;

}