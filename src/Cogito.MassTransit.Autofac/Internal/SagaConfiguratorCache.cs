using System;
using System.Collections.Concurrent;

using Autofac;

using MassTransit;
using MassTransit.Saga;

namespace Cogito.MassTransit.Autofac.Internal
{

    static class SagaConfiguratorCache
    {

        static ICachedConfigurator GetOrAdd(Type type)
        {
            return Cached.Instance.GetOrAdd(type, _ => (ICachedConfigurator)Activator.CreateInstance(typeof(CachedConfigurator<>).MakeGenericType(type)));
        }

        public static void Configure(Type sagaType, IReceiveEndpointConfigurator configurator, IComponentContext context)
        {
            GetOrAdd(sagaType).Configure(configurator, context);
        }


        interface ICachedConfigurator
        {

            void Configure(IReceiveEndpointConfigurator configurator, IComponentContext context);

        }


        class CachedConfigurator<TSaga> : ICachedConfigurator
            where TSaga : class, ISaga
        {

            public void Configure(IReceiveEndpointConfigurator configurator, IComponentContext context)
            {
                configurator.Saga<TSaga>(context);
            }

        }

        static class Cached
        {

            internal static readonly ConcurrentDictionary<Type, ICachedConfigurator> Instance = new ConcurrentDictionary<Type, ICachedConfigurator>();

        }

    }

}
