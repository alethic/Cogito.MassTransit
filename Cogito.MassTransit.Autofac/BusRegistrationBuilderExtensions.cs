﻿using System;

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
    public static class BusRegistrationBuilderExtensions
    {

        /// <summary>
        /// Adds configuration to the named bus.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static BusRegistrationBuilder Configure(this BusRegistrationBuilder builder, Action<IBusFactoryConfigurator> configuration)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));
            if (configuration is null)
                throw new ArgumentNullException(nameof(configuration));

            builder.Builder.RegisterInstance(new DelegateBusConfiguration(builder.Name, configuration)).As<IBusConfiguration>();
            return builder;
        }

        /// <summary>
        /// Adds configuration to the named bus.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static BusRegistrationBuilder Configure(this BusRegistrationBuilder builder, Action<IComponentContext, IBusFactoryConfigurator> configuration)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));
            if (configuration is null)
                throw new ArgumentNullException(nameof(configuration));

            builder.Builder.Register(context => { var ctx = context.Resolve<IComponentContext>(); return new DelegateBusConfiguration(builder.Name, configurator => configuration(ctx, configurator)); }).As<IBusConfiguration>();
            return builder;
        }

        /// <summary>
        /// Adds message scheduling support.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="schedulerAddress"></param>
        /// <returns></returns>
        public static BusRegistrationBuilder UseMessageScheduler(this BusRegistrationBuilder builder, Uri schedulerAddress)
        {
            return builder.Configure(c => c.UseMessageScheduler(schedulerAddress));
        }

        /// <summary>
        /// Adds message scheduling support.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="endpointName"></param>
        /// <returns></returns>
        public static BusRegistrationBuilder UseMessageScheduler(this BusRegistrationBuilder builder, string endpointName)
        {
            return builder.Configure(c => c.UseMessageScheduler(new Uri("queue:" + endpointName)));
        }

        /// <summary>
        /// Adds message scheduling support.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static BusRegistrationBuilder UsePublishMessageScheduler(this BusRegistrationBuilder builder)
        {
            return builder.Configure(c => c.UsePublishMessageScheduler());
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
