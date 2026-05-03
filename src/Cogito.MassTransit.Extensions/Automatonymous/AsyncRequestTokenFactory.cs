using System.Threading.Tasks;

using Automatonymous;

using MassTransit;

namespace Cogito.MassTransit.Automatonymous
{

    public delegate Task<IRequestToken<TRequest>> AsyncRequestTokenFactory<in TSaga, in TMessage, TRequest>(SagaConsumeContext<TSaga, TMessage> context)
        where TSaga : class, ISaga
        where TMessage : class;

    public delegate Task<IRequestToken<TRequest>> AsyncRequestTokenFactory<in TSaga, TRequest>(SagaConsumeContext<TSaga> context)
        where TSaga : class, ISaga;

}
