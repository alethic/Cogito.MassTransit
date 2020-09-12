using Automatonymous;

using MassTransit;

namespace Cogito.MassTransit.Automatonymous
{

    public delegate ExceptionInfo ExceptionFactory<in TInstance, in TData, TRequest>(ConsumeEventContext<TInstance, TData> context)
        where TData : class
        where TInstance : class, SagaStateMachineInstance;

}