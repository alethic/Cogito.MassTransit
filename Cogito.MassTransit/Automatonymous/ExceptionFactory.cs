using System;

using Automatonymous;

namespace Cogito.MassTransit.Automatonymous
{

    public delegate Exception ExceptionFactory<in TInstance, in TData, TRequest>(ConsumeEventContext<TInstance, TData> context)
        where TData : class
        where TInstance : class, SagaStateMachineInstance;

}