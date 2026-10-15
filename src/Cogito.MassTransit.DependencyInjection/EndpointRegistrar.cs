using System;

using MassTransit;
using MassTransit.Configuration;

using Microsoft.Extensions.DependencyInjection;

namespace Cogito.MassTransit.DependencyInjection
{

    /// <summary>
    /// Registers a receive endpoint for a registration, with the settings given by an attribute.
    /// </summary>
    static class EndpointRegistrar
    {

        /// <summary>
        /// Registers the endpoint described by <paramref name="endpointDefinitionType"/>, which closes
        /// <see cref="IEndpointDefinition{T}"/> over <paramref name="endpointType"/>, for <paramref name="registration"/>.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="registrar"></param>
        /// <param name="registration"></param>
        /// <param name="endpointDefinitionType"></param>
        /// <param name="endpointType"></param>
        /// <param name="settings"></param>
        public static void RegisterEndpoint(IServiceCollection services, IContainerRegistrar registrar, IRegistration registration, Type endpointDefinitionType, Type endpointType, EndpointOptions settings)
        {
            var register = (IRegister)Activator.CreateInstance(typeof(Register<,>).MakeGenericType(endpointDefinitionType, endpointType))!;
            register.RegisterEndpoint(services, registrar, registration, settings);
        }

        interface IRegister
        {

            void RegisterEndpoint(IServiceCollection services, IContainerRegistrar registrar, IRegistration registration, EndpointOptions options);

        }

        class Register<TDefinition, T> : IRegister
            where TDefinition : class, IEndpointDefinition<T>
            where T : class
        {

            public void RegisterEndpoint(IServiceCollection services, IContainerRegistrar registrar, IRegistration registration, EndpointOptions options)
            {
                var settings = new EndpointSettings<IEndpointDefinition<T>>
                {
                    Name = options.Name,
                    IsTemporary = options.IsTemporary,
                    InstanceId = options.InstanceId,
                    ConfigureConsumeTopology = options.ConfigureConsumeTopology,
                };

                if (options.PrefetchCount != 0)
                    settings.PrefetchCount = options.PrefetchCount;
                if (options.ConcurrentMessageLimit != 0)
                    settings.ConcurrentMessageLimit = options.ConcurrentMessageLimit;

                services.RegisterEndpoint<TDefinition, T>(registrar, registration, settings);
            }

        }

    }

    /// <summary>
    /// The endpoint settings an attribute gives.
    /// </summary>
    class EndpointOptions
    {

        public string? Name { get; set; }

        public int PrefetchCount { get; set; }

        public int ConcurrentMessageLimit { get; set; }

        public bool IsTemporary { get; set; }

        public string? InstanceId { get; set; }

        public bool ConfigureConsumeTopology { get; set; } = true;

    }

}
