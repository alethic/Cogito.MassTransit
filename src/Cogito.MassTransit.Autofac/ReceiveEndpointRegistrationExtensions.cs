using System;

using Autofac;
using Autofac.Builder;

using Cogito.MassTransit.Registration;

namespace Cogito.MassTransit.Autofac
{

    /// <summary>
    /// Provides extensions to register receive endpoint configurations.
    /// </summary>
    public static class ReceiveEndpointRegistrationExtensions
    {

        /// <summary>
        /// Registers a receive endpoint configuration for a bus and endpoint.
        /// </summary>
        /// <typeparam name="TConfiguration"></typeparam>
        /// <param name="builder"></param>
        /// <param name="busName"></param>
        /// <param name="endpointName"></param>
        /// <returns></returns>
        public static IRegistrationBuilder<TConfiguration, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterReceiveEndpointConfiguration<TConfiguration>(this ContainerBuilder builder, string busName, string endpointName)
            where TConfiguration : IReceiveEndpointConfiguration
        {
            return builder.RegisterType<TConfiguration>().As<IReceiveEndpointConfiguration>().WithMetadata<IReceiveEndpointMetadata>(i => i.For(j => j.BusName, busName).For(j => j.EndpointName, endpointName));
        }

        /// <summary>
        /// Registers a receive endpoint configuration for a bus and endpoint.
        /// </summary>
        /// <typeparam name="TConfiguration"></typeparam>
        /// <param name="builder"></param>
        /// <param name="busName"></param>
        /// <param name="endpointName"></param>
        /// <returns></returns>
        public static IRegistrationBuilder<TConfiguration, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterReceiveEndpointConfiguration<TConfiguration>(this ContainerBuilder builder, string endpointName)
            where TConfiguration : IReceiveEndpointConfiguration
        {
            return RegisterReceiveEndpointConfiguration<TConfiguration>(builder, "", endpointName);
        }

        /// <summary>
        /// Registers a receive endpoint configuration for a bus and endpoint.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="type"></param>
        /// <param name="busName"></param>
        /// <param name="endpointName"></param>
        /// <returns></returns>
        public static IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterReceiveEndpointConfiguration(this ContainerBuilder builder, Type type, string busName, string endpointName)
        {
            return builder.RegisterType(type).As<IReceiveEndpointConfiguration>().WithMetadata<IReceiveEndpointMetadata>(i => i.For(j => j.BusName, busName).For(j => j.EndpointName, endpointName));
        }

        /// <summary>
        /// Registers a receive endpoint configuration for a bus and endpoint.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="type"></param>
        /// <param name="busName"></param>
        /// <param name="endpointName"></param>
        /// <returns></returns>
        public static IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterReceiveEndpointConfiguration(this ContainerBuilder builder, Type type, string endpointName)
        {
            return RegisterReceiveEndpointConfiguration(builder, type, "", endpointName);
        }

    }

}
