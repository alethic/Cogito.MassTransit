using System;
using System.Collections.Concurrent;

using MassTransit;
using MassTransit.Scoping;

namespace Cogito.MassTransit.Registration.Internal
{

    static class StateMachineSagaConfiguratorCache
    {

        static ICachedConfigurator GetOrAdd(Type type)
        {
            return Cached.Instance.GetOrAdd(type, _ => (ICachedConfigurator)Activator.CreateInstance(typeof(CachedConfigurator<>).MakeGenericType(type)));
        }

        public static void Configure(Type sagaType, IReceiveEndpointConfigurator configurator, IConsumerScopeProvider scopeProvider)
        {
            GetOrAdd(sagaType).Configure(configurator, scopeProvider);
        }


        interface ICachedConfigurator
        {

            void Configure(IReceiveEndpointConfigurator configurator, IConsumerScopeProvider scopeProvider);

        }


        class CachedConfigurator<T> : ICachedConfigurator
            where T : class, IConsumer
        {

            public void Configure(IReceiveEndpointConfigurator configurator, IConsumerScopeProvider scopeProvider)
            {
                var consumerFactory = new ScopeConsumerFactory<T>(scopeProvider);
                configurator.Consumer(consumerFactory);
            }

        }

        static class Cached
        {

            internal static readonly ConcurrentDictionary<Type, ICachedConfigurator> Instance = new ConcurrentDictionary<Type, ICachedConfigurator>();

        }

    }

}
