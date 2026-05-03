using System;

using Automatonymous;

using MassTransit;

namespace Cogito.MassTransit.Automatonymous
{

    public delegate Exception ExceptionFactory<in TSaga, in TMessage, TRequest>(SagaConsumeContext<TSaga, TMessage> context)
        where TSaga : class, ISaga
        where TMessage : class;

}