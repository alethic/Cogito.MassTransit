using System;

using Cogito.DependencyInjection;

using MassTransit;
using MassTransit.Configuration;
using MassTransit.DependencyInjection;
using MassTransit.DependencyInjection.Registration;

using Microsoft.Extensions.DependencyInjection;

namespace Cogito.MassTransit.DependencyInjection
{

    /// <summary>
    /// Adds the decorated <see cref="IConsumer"/> to the service collection to be registered on the specified typed bus.
    /// </summary>
    public class AddConsumerAttribute<TBus> : Attribute, IServiceRegistrationAttribute
        where TBus : IBus
    {

        /// <summary>
        /// Settings available to the attribute.
        /// </summary>
        class AddConsumerAttributeEndpointSettings<T> : EndpointSettings<T>
            where T : class
        {

            /// <summary>
            /// Initializes a new instance.
            /// </summary>
            /// <param name="name"></param>
            public AddConsumerAttributeEndpointSettings(string? name)
            {
                if (name != null)
                    Name = name;
            }

        }

        /// <summary>
        /// Gets or sets the name of the endpoint on which to register the consumer.
        /// </summary>
        public string? EndpointName { get; set; }

        /// <inheritdoc />
        public int RegistrationOrder => 0;

        /// <inheritdoc />
        public void Add(IServiceCollection services, Type implementationType)
        {
            var c = new DependencyInjectionContainerRegistrar(services);
            var r = services.RegisterConsumer(c, implementationType);
            if (EndpointName != null)
            {
                var endpointDefinitionType = typeof(Bind<,>).MakeGenericType(typeof(TBus), typeof(IEndpointDefinition<>).MakeGenericType(implementationType));
                services.AddTransient(endpointDefinitionType, typeof(ConsumerEndpointDefinition<>).MakeGenericType(implementationType));
                services.AddSingleton(typeof(IEndpointSettings<>).MakeGenericType(endpointDefinitionType), Activator.CreateInstance(typeof(AddConsumerAttributeEndpointSettings<>).MakeGenericType(typeof(TBus), endpointDefinitionType), [EndpointName]) ?? throw new InvalidOperationException());
                services.AddSingleton(typeof(IEndpointRegistration), Activator.CreateInstance(typeof(EndpointRegistration<>).MakeGenericType(implementationType), [r]) ?? throw new InvalidOperationException());
            }
        }

    }

    /// <summary>
    /// Adds the decorated <see cref="IConsumer"/> to the service collection.
    /// </summary>
    public class AddConsumerAttribute : Attribute, IServiceRegistrationAttribute
    {

        /// <summary>
        /// Settings available to the attribute.
        /// </summary>
        class AddConsumerAttributeEndpointSettings<T> : EndpointSettings<T>
            where T : class
        {

            /// <summary>
            /// Initializes a new instance.
            /// </summary>
            /// <param name="name"></param>
            public AddConsumerAttributeEndpointSettings(string? name)
            {
                if (name != null)
                    Name = name;
            }

        }

        /// <summary>
        /// Gets or sets the name of the endpoint on which to register the consumer.
        /// </summary>
        public string? EndpointName { get; set; }

        /// <inheritdoc />
        public int RegistrationOrder => 0;

        /// <inheritdoc />
        public void Add(IServiceCollection services, Type implementationType)
        {
            var c = new DependencyInjectionContainerRegistrar(services);
            var r = services.RegisterConsumer(c, implementationType);
            if (EndpointName != null)
            {
                var endpointDefinitionType = typeof(IEndpointDefinition<>).MakeGenericType(implementationType);
                services.AddTransient(endpointDefinitionType, typeof(ConsumerEndpointDefinition<>).MakeGenericType(implementationType));
                services.AddSingleton(typeof(IEndpointSettings<>).MakeGenericType(endpointDefinitionType), Activator.CreateInstance(typeof(AddConsumerAttributeEndpointSettings<>).MakeGenericType(endpointDefinitionType), [EndpointName]) ?? throw new InvalidOperationException());
                services.AddSingleton(typeof(IEndpointRegistration), Activator.CreateInstance(typeof(EndpointRegistration<>).MakeGenericType(implementationType), [r]) ?? throw new InvalidOperationException());
            }
        }

    }

}
