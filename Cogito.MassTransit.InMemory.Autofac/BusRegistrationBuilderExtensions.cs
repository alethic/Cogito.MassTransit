using System;

using Autofac;

using Cogito.Autofac.DependencyInjection;
using Cogito.MassTransit.Autofac;
using Cogito.MassTransit.InMemory.Registration;
using Cogito.MassTransit.Registration;

using MassTransit;

using Microsoft.Extensions.DependencyInjection;

namespace Cogito.MassTransit.InMemory.Autofac
{

    /// <summary>
    /// Provides extension methods for further configuration of a bus.
    /// </summary>
    public static class BusRegistrationBuilderExtensions
    {

        /// <summary>
        /// Registers the bus as an InMemory Bus.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        static void RegisterDefinition(this BusRegistrationBuilder builder)
        {
            // we need to register the bus definition only once
            if (builder.Builder.Properties.ContainsKey($"{typeof(BusDefinition)}::{builder.Name}") == false)
            {
                var i = new BusDefinition(builder.Name, Bus.Factory.CreateUsingInMemory);
                builder.Builder.RegisterInstance(i);
                builder.Builder.Properties.Add($"{typeof(BusDefinition)}::{builder.Name}", i);
            }
        }

        /// <summary>
        /// Registers the bus as an InMemory Bus.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static BusRegistrationBuilder UsingInMemoryBus(this BusRegistrationBuilder builder, Action<InMemoryBusOptions> configure)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));
            if (configure is null)
                throw new ArgumentNullException(nameof(configure));

            RegisterDefinition(builder);
            builder.Builder.Populate(c => c.Configure(builder.Name, configure));
            return builder;
        }

        /// <summary>
        /// Registers the bus as an InMemory Bus.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static BusRegistrationBuilder UsingInMemoryBus(this BusRegistrationBuilder builder)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

            return UsingInMemoryBus(builder, o => { });
        }

    }

}
