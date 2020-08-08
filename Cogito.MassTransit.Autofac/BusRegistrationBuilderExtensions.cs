using System;

using Autofac;

using Cogito.MassTransit.Registration;

using Microsoft.Extensions.Hosting;

namespace Cogito.MassTransit.Autofac
{

    /// <summary>
    /// Provides extension methods for further configuration of a bus.
    /// </summary>
    public static class BusRegistrationBuilderExtensions
    {

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
