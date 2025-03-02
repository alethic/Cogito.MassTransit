using Automatonymous;

namespace Cogito.MassTransit.Automatonymous
{

    public delegate IRequestToken<TRequest> RequestTokenFactory<in TInstance, in TData, TRequest>(EventContext<TInstance, TData> context);

    public delegate IRequestToken<TRequest> RequestTokenFactory<in TInstance, TRequest>(EventContext<TInstance> context);

}
