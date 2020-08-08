using System;

using Autofac;
using Autofac.Builder;

using Cogito.MassTransit.Registration;

using MassTransit;

namespace Cogito.MassTransit.Autofac
{

    /// <summary>
    /// Provides extensions to register consumers.
    /// </summary>
    public static class ConsumerRegistrationExtensions
    {

        /// <summary>
        /// Registers a consumer on a bus and endpoint.
        /// </summary>
        /// <typeparam name="TConsumer"></typeparam>
        /// <param name="builder"></param>
        /// <param name="busName"></param>
        /// <param name="endpointName"></param>
        /// <returns></returns>
        public static IRegistrationBuilder<TConsumer, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterConsumer<TConsumer>(this ContainerBuilder builder, string busName, string endpointName)
            where TConsumer : IConsumer
        {
            return builder.RegisterType<TConsumer>().WithMetadata<IReceiveEndpointMetadata>(i => i.For(j => j.BusName, busName).For(j => j.EndpointName, endpointName));
        }

        /// <summary>
        /// Registers a consumer on an endpoint with the default bus.
        /// </summary>
        /// <typeparam name="TConsumer"></typeparam>
        /// <param name="builder"></param>
        /// <param name="busName"></param>
        /// <param name="endpointName"></param>
        /// <returns></returns>
        public static IRegistrationBuilder<TConsumer, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterConsumer<TConsumer>(this ContainerBuilder builder, string endpointName)
            where TConsumer : IConsumer
        {
            return RegisterConsumer<TConsumer>(builder, "", endpointName);
        }

        /// <summary>
        /// Registers a consumer on a bus and endpoint.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="type"></param>
        /// <param name="busName"></param>
        /// <param name="endpointName"></param>
        /// <returns></returns>
        public static IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterConsumer(this ContainerBuilder builder, Type type, string busName, string endpointName)
        {
            return builder.RegisterType(type).WithMetadata<IReceiveEndpointMetadata>(i => i.For(j => j.BusName, busName).For(j => j.EndpointName, endpointName));
        }

        /// <summary>
        /// Registers a consumer on an endpoint with the default bus.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="type"></param>
        /// <param name="busName"></param>
        /// <param name="endpointName"></param>
        /// <returns></returns>
        public static IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterConsumer(this ContainerBuilder builder, Type type, string endpointName)
        {
            return RegisterConsumer(builder, type, "", endpointName);
        }

    }

}
