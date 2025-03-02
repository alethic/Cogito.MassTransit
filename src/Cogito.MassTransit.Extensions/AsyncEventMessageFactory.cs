using System.Threading.Tasks;

using MassTransit.Contracts;

namespace Cogito.MassTransit
{

    public delegate Task<TMessage> AsyncEventMessageFactory<in TInstance, in TData, TMessage>(EventContext<TInstance, TData> context);

    public delegate Task<TMessage> AsyncEventMessageFactory<in TInstance, TMessage>(EventContext<TInstance> context);

}
