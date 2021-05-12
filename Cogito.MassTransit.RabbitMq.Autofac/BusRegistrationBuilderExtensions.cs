using System;

using Autofac;

using Cogito.Extensions.Options.Autofac;
using Cogito.MassTransit.Autofac;
using Cogito.MassTransit.RabbitMq.Registration;
using Cogito.MassTransit.Registration;

using MassTransit;

using Microsoft.Extensions.DependencyInjection;

namespace Cogito.MassTransit.RabbitMq.Autofac
{

    /// <summary>
    /// Provides extension methods for further configuration of a bus.
    /// </summary>
    public static class BusRegistrationBuilderExtensions
    {

        /// <summary>
        /// Registers the bus as a RabbitMQ Bus.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        static void RegisterDefinition(this BusRegistrationBuilder builder)
        {
            // we need to register the bus definition only once
            if (builder.Builder.Properties.ContainsKey($"{typeof(BusDefinition)}::{builder.Name}") == false)
            {
                var i = new BusDefinition(builder.Name, Bus.Factory.CreateUsingRabbitMq);
                builder.Builder.RegisterInstance(i);
                builder.Builder.Properties.Add($"{typeof(BusDefinition)}::{builder.Name}", i);
            }
        }

        /// <summary>
        /// Registers the bus as a RabbitMQ Bus.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static BusRegistrationBuilder UsingRabbitMq(this BusRegistrationBuilder builder, Action<RabbitMqBusOptions> configure)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));
            if (configure is null)
                throw new ArgumentNullException(nameof(configure));

            RegisterDefinition(builder);
            builder.Builder.Configure(builder.Name, configure);
            return builder;
        }

        /// <summary>
        /// Registers the bus as a RabbitMQ bus.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static BusRegistrationBuilder UsingRabbitMq(this BusRegistrationBuilder builder)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

            return UsingRabbitMq(builder, o => { });
        }

    }

}
