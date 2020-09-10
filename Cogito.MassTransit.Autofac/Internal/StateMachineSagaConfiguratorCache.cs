using System;
using System.Collections.Concurrent;

using Autofac;

using Automatonymous;

using MassTransit;
using MassTransit.Internals.Extensions;

namespace Cogito.MassTransit.Autofac.Internal
{

    static class StateMachineSagaConfiguratorCache
    {

        static ICachedConfigurator GetOrAdd(Type type)
        {
            return Cached.Instance.GetOrAdd(type, _ => (ICachedConfigurator)Activator.CreateInstance(typeof(CachedConfigurator<>).MakeGenericType(type.GetClosingArgument(typeof(SagaStateMachine<>)))));
        }

        public static void Configure(Type sagaType, IReceiveEndpointConfigurator configurator, IComponentContext context)
        {
            GetOrAdd(sagaType).Configure(configurator, context);
        }


        interface ICachedConfigurator
        {

            void Configure(IReceiveEndpointConfigurator configurator, IComponentContext context);

        }


        class CachedConfigurator<TInstance> : ICachedConfigurator
            where TInstance : class, SagaStateMachineInstance
        {

            public void Configure(IReceiveEndpointConfigurator configurator, IComponentContext context)
            {
                configurator.StateMachineSaga<TInstance>(context);
            }

        }

        static class Cached
        {

            internal static readonly ConcurrentDictionary<Type, ICachedConfigurator> Instance = new ConcurrentDictionary<Type, ICachedConfigurator>();

        }

    }

}
