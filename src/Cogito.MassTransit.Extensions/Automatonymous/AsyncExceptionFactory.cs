using System;
using System.Threading.Tasks;

using MassTransit;

namespace Cogito.MassTransit.Automatonymous
{

    public delegate Task<Exception> AsyncExceptionFactory<in TSaga, in TMessage, TRequest>(SagaConsumeContext<TSaga, TMessage> context)
        where TSaga : class, ISaga
        where TMessage : class;

}