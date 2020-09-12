using System;
using System.Threading.Tasks;

using Automatonymous;

namespace Cogito.MassTransit.Automatonymous
{

    public delegate Task<Exception> AsyncExceptionFactory<in TInstance, in TData, TRequest>(ConsumeEventContext<TInstance, TData> context)
        where TData : class
        where TInstance : class, SagaStateMachineInstance;

}