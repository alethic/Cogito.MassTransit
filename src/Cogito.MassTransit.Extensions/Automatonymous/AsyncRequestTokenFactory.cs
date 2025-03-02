using System.Threading.Tasks;

using Automatonymous;

namespace Cogito.MassTransit.Automatonymous
{

    public delegate Task<IRequestToken<TRequest>> AsyncRequestTokenFactory<in TInstance, in TData, TRequest>(EventContext<TInstance, TData> context);

    public delegate Task<IRequestToken<TRequest>> AsyncRequestTokenFactory<in TInstance, TRequest>(EventContext<TInstance> context);

}
