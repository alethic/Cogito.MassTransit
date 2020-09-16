using System;

using Autofac;

namespace Cogito.MassTransit.Autofac
{

    public static class BusRegistrationExtensions
    {

        /// <summary>
        /// Registers a MassTransit bus.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="name"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static ContainerBuilder RegisterMassTransitBus(this ContainerBuilder builder, string name, Action<BusRegistrationBuilder> configure)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));
            if (name is null)
                throw new ArgumentNullException(nameof(name));
            if (configure is null)
                throw new ArgumentNullException(nameof(configure));

            builder.RegisterModule<AssemblyModule>();
            configure(new BusRegistrationBuilder(builder, name));
            return builder;
        }

        /// <summary>
        /// Registers a MassTransit bus.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static ContainerBuilder RegisterMassTransitBus(this ContainerBuilder builder, Action<BusRegistrationBuilder> configure)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));
            if (configure is null)
                throw new ArgumentNullException(nameof(configure));

            return RegisterMassTransitBus(builder, "", configure);
        }

    }

}
