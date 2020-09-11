using System.Threading.Tasks;

using Automatonymous;

namespace Cogito.MassTransit.Automatonymous
{

    public delegate Task<IRequestToken<TRequest>> AsyncRequestTokenFactory<in TInstance, in TData, TRequest>(ConsumeEventContext<TInstance, TData> context)
        where TData : class;

}
