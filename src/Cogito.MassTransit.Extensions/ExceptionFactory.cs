using System;

using MassTransit;

namespace Cogito.MassTransit
{

    public delegate Exception ExceptionFactory<in TSaga, in TMessage, TRequest>(SagaConsumeContext<TSaga, TMessage> context)
        where TSaga : class, ISaga
        where TMessage : class;

}