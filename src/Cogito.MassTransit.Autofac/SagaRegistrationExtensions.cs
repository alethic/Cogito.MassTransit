using System;

using Autofac;
using Autofac.Builder;

using Cogito.MassTransit.Registration;

using MassTransit.Saga;

namespace Cogito.MassTransit.Autofac
{

    /// <summary>
    /// Provides extensions to register sagas.
    /// </summary>
    public static class SagaRegistrationExtensions
    {

        /// <summary>
        /// Registers a saga on a bus and endpoint.
        /// </summary>
        /// <typeparam name="TSaga"></typeparam>
        /// <param name="builder"></param>
        /// <param name="busName"></param>
        /// <param name="endpointName"></param>
        /// <returns></returns>
        public static IRegistrationBuilder<TSaga, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterSaga<TSaga>(this ContainerBuilder builder, string busName, string endpointName)
            where TSaga : ISaga
        {
            return builder.RegisterType<TSaga>().WithMetadata<IReceiveEndpointMetadata>(i => i.For(j => j.BusName, busName).For(j => j.EndpointName, endpointName));
        }

        /// <summary>
        /// Registers a consumer on an endpoint with the default bus.
        /// </summary>
        /// <typeparam name="TSaga"></typeparam>
        /// <param name="builder"></param>
        /// <param name="busName"></param>
        /// <param name="endpointName"></param>
        /// <returns></returns>
        public static IRegistrationBuilder<TSaga, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterSaga<TSaga>(this ContainerBuilder builder, string endpointName)
            where TSaga : ISaga
        {
            return RegisterSaga<TSaga>(builder, "", endpointName);
        }

        /// <summary>
        /// Registers a consumer on a bus and endpoint.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="type"></param>
        /// <param name="busName"></param>
        /// <param name="endpointName"></param>
        /// <returns></returns>
        public static IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterSaga(this ContainerBuilder builder, Type type, string busName, string endpointName)
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
        public static IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterSaga(this ContainerBuilder builder, Type type, string endpointName)
        {
            return RegisterSaga(builder, type, "", endpointName);
        }

    }

}
