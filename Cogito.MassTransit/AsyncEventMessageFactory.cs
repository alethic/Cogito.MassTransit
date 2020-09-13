using System.Threading.Tasks;

using Automatonymous;

namespace Cogito.MassTransit
{

    public delegate Task<TMessage> AsyncEventMessageFactory<in TInstance, in TData, TMessage>(EventContext<TInstance, TData> context);

    public delegate Task<TMessage> AsyncEventMessageFactory<in TInstance, TMessage>(EventContext<TInstance> context);

}
