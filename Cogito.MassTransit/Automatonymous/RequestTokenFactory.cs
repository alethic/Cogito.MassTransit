using Automatonymous;

namespace Cogito.MassTransit.Automatonymous
{

    public delegate IRequestToken<TRequest> RequestTokenFactory<in TInstance, in TData, TRequest>(ConsumeEventContext<TInstance, TData> context)
        where TData : class;

}   
