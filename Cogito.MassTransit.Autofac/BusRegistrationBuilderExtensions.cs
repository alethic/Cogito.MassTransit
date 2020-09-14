using System;

using Autofac;

using Cogito.MassTransit.Hosting;
using Cogito.MassTransit.Registration;

using MassTransit;

using Microsoft.Extensions.Hosting;

namespace Cogito.MassTransit.Autofac
{

    /// <summary>
    /// Provides extension methods for further configuration of a bus.
    /// </summary>
    public static partial class BusRegistrationBuilderExtensions
    {

        /// <summary>
        /// Adds configuration to the default bus.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static BusRegistrationBuilder Configure(this BusRegistrationBuilder builder, Action<IBusFactoryConfigurator> configuration)
        {
            return Configure(builder, "", configuration);
        }

        /// <summary>
        /// Adds configuration to the default bus.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static BusRegistrationBuilder Configure(this BusRegistrationBuilder builder, Action<IComponentContext, IBusFactoryConfigurator> configuration)
        {
            return Configure(builder, "", configuration);
        }

        /// <summary>
        /// Adds configuration to the named bus.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="busName"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static BusRegistrationBuilder Configure(this BusRegistrationBuilder builder, string busName, Action<IBusFactoryConfigurator> configuration)
        {
            builder.Builder.RegisterInstance(new DelegateBusConfiguration(busName, configuration));
            return builder;
        }

        /// <summary>
        /// Adds configuration to the named bus.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="busName"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static BusRegistrationBuilder Configure(this BusRegistrationBuilder builder, string busName, Action<IComponentContext, IBusFactoryConfigurator> configuration)
        {
            builder.Builder.Register(context => { var ctx = context.Resolve<IComponentContext>(); new DelegateBusConfiguration(busName, configurator => configuration(ctx, configurator)));
            return builder;
        }

        /// <summary>
        /// Includes a <see cref="IHostedService"/> implementation along with the bus.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static BusRegistrationBuilder WithHostedService(this BusRegistrationBuilder builder)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

            builder.Builder.Register(ctx => new BusHostedService(ctx.Resolve<BusProvider>().GetBus(builder.Name))).As<IHostedService>().SingleInstance();
            return builder;
        }

    }

}
