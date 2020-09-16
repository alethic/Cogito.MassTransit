using System;

using Autofac;
using Autofac.Builder;

using Automatonymous;

using Cogito.MassTransit.Registration;

using MassTransit.Internals.Extensions;

namespace Cogito.MassTransit.Autofac
{

    /// <summary>
    /// Provides extensions to register state machine sagas.
    /// </summary>
    public static class SagaStateMachineRegistrationExtensions
    {

        /// <summary>
        /// Registers a state machine saga on a bus and endpoint.
        /// </summary>
        /// <typeparam name="TStateMachine"></typeparam>
        /// <typeparam name="TInstance"></typeparam>
        /// <param name="builder"></param>
        /// <param name="busName"></param>
        /// <param name="endpointName"></param>
        /// <returns></returns>
        public static IRegistrationBuilder<TStateMachine, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterSagaStateMachine<TStateMachine, TInstance>(this ContainerBuilder builder, string busName, string endpointName)
            where TStateMachine : class, SagaStateMachine<TInstance>
            where TInstance : class, SagaStateMachineInstance
        {
            builder.RegisterModule<AssemblyModule>();

            return builder.RegisterType<TStateMachine>()
                .AsSelf()
                .As<SagaStateMachine<TInstance>>()
                .WithMetadata<IReceiveEndpointMetadata>(i => i.For(j => j.BusName, busName).For(j => j.EndpointName, endpointName));
        }

        /// <summary>
        /// Registers a state machine saga on an endpoint with the default bus.
        /// </summary>
        /// <typeparam name="TStateMachine"></typeparam>
        /// <typeparam name="TInstance"></typeparam>
        /// <param name="builder"></param>
        /// <param name="busName"></param>
        /// <param name="endpointName"></param>
        /// <returns></returns>
        public static IRegistrationBuilder<TStateMachine, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterSagaStateMachine<TStateMachine, TInstance>(this ContainerBuilder builder, string endpointName)
            where TStateMachine : class, SagaStateMachine<TInstance>
            where TInstance : class, SagaStateMachineInstance
        {
            return RegisterSagaStateMachine<TStateMachine, TInstance>(builder, "", endpointName);
        }

        /// <summary>
        /// Registers a state machine saga on a bus and endpoint.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="type"></param>
        /// <param name="busName"></param>
        /// <param name="endpointName"></param>
        /// <returns></returns>
        public static IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterSagaStateMachine(this ContainerBuilder builder, Type type, string busName, string endpointName)
        {
            builder.RegisterModule<AssemblyModule>();

            return builder.RegisterType(type)
                .AsSelf()
                .As(typeof(SagaStateMachine<>).MakeGenericType(type.GetClosingArgument(typeof(SagaStateMachine<>))))
                .WithMetadata<IReceiveEndpointMetadata>(i => i.For(j => j.BusName, busName).For(j => j.EndpointName, endpointName));
        }

        /// <summary>
        /// Registers a state machine saga on an endpoint with the default bus.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="type"></param>
        /// <param name="busName"></param>
        /// <param name="endpointName"></param>
        /// <returns></returns>
        public static IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterSagaStateMachine(this ContainerBuilder builder, Type type, string endpointName)
        {
            return RegisterSagaStateMachine(builder, type, "", endpointName);
        }

    }

}
