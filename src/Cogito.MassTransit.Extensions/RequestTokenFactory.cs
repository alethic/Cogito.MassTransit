using MassTransit;

namespace Cogito.MassTransit
{

    public delegate IRequestToken<TRequest>? RequestTokenFactory<in TSaga, in TMessage, TRequest>(SagaConsumeContext<TSaga, TMessage> context)
        where TSaga : class, ISaga
        where TMessage : class;

    public delegate IRequestToken<TRequest>? RequestTokenFactory<in TSaga, TRequest>(SagaConsumeContext<TSaga> context)
        where TSaga : class, ISaga;

}
