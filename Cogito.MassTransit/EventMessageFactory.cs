using Automatonymous;

namespace Cogito.MassTransit
{

    public delegate TMessage EventMessageFactory<in TInstance, in TData, out TMessage>(EventContext<TInstance, TData> context);

    public delegate TMessage EventMessageFactory<in TInstance, out TMessage>(EventContext<TInstance> context);

}
