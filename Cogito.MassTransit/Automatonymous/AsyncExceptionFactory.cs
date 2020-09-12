using System.Threading.Tasks;

using Automatonymous;

using MassTransit;

namespace Cogito.MassTransit.Automatonymous
{

    public delegate Task<ExceptionInfo> AsyncExceptionFactory<in TInstance, in TData, TRequest>(ConsumeEventContext<TInstance, TData> context)
        where TData : class
        where TInstance : class, SagaStateMachineInstance;

}