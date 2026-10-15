using System;

using MassTransit;
using MassTransit.Configuration;

using Microsoft.Extensions.DependencyInjection;

namespace Cogito.MassTransit.DependencyInjection
{

    /// <summary>
    /// Base for attributes that register a type which receives on its own endpoint, and can configure that
    /// endpoint. Endpoint properties that are left unset fall to the type's definition, or to the bus's endpoint
    /// name formatter.
    /// </summary>
    public abstract class EndpointRegistrationAttribute : MassTransitRegistrationAttribute
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        protected EndpointRegistrationAttribute()
        {
            ConfigureConsumeTopology = DefaultConfigureConsumeTopology;
        }

        /// <summary>
        /// Whether the endpoint subscribes to the types it consumes, unless set otherwise.
        /// </summary>
        protected virtual bool DefaultConfigureConsumeTopology => true;

        /// <summary>
        /// The name of the receive endpoint.
        /// </summary>
        public string? EndpointName { get; set; }

        /// <summary>
        /// The number of messages to fetch from the broker ahead of being consumed. Zero leaves the transport's default.
        /// </summary>
        public int PrefetchCount { get; set; }

        /// <summary>
        /// The number of messages consumed at once. Zero leaves the transport's default.
        /// </summary>
        public int ConcurrentMessageLimit { get; set; }

        /// <summary>
        /// Whether the endpoint is removed when the bus stops.
        /// </summary>
        public bool IsTemporary { get; set; }

        /// <summary>
        /// Distinguishes the endpoint name per instance, for endpoints each instance should receive on.
        /// </summary>
        public string? InstanceId { get; set; }

        /// <summary>
        /// Whether the endpoint subscribes to the types it consumes.
        /// </summary>
        public bool ConfigureConsumeTopology { get; set; }

        /// <summary>
        /// Leaves the type off the endpoints created by <c>ConfigureEndpoints</c>, so it can be configured onto an
        /// endpoint by hand. Endpoint properties cannot be set with this.
        /// </summary>
        public bool ExcludeFromConfigureEndpoints { get; set; }

        /// <summary>
        /// Whether any endpoint property was set.
        /// </summary>
        bool HasEndpointSettings =>
            EndpointName != null ||
            PrefetchCount != 0 ||
            ConcurrentMessageLimit != 0 ||
            IsTemporary ||
            InstanceId != null ||
            ConfigureConsumeTopology != DefaultConfigureConsumeTopology;

        /// <summary>
        /// Applies the endpoint properties to the registration: either excludes it from <c>ConfigureEndpoints</c>,
        /// or, if any were set, registers an endpoint for it with them.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="registrar"></param>
        /// <param name="registration"></param>
        /// <param name="endpointDefinitionType">The endpoint definition, which closes <see cref="IEndpointDefinition{T}"/> over <paramref name="endpointType"/>.</param>
        /// <param name="endpointType"></param>
        /// <param name="implementationType"></param>
        protected void ApplyEndpoint(IServiceCollection services, IContainerRegistrar registrar, IRegistration registration, Type endpointDefinitionType, Type endpointType, Type implementationType)
        {
            if (ExcludeFromConfigureEndpoints)
            {
                if (HasEndpointSettings)
                    throw new InvalidOperationException($"{implementationType} is excluded from ConfigureEndpoints, so its endpoint cannot be configured.");

                registration.IncludeInConfigureEndpoints = false;
                return;
            }

            if (HasEndpointSettings)
                EndpointRegistrar.RegisterEndpoint(services, registrar, registration, endpointDefinitionType, endpointType, new EndpointOptions()
                {
                    Name = EndpointName,
                    PrefetchCount = PrefetchCount,
                    ConcurrentMessageLimit = ConcurrentMessageLimit,
                    IsTemporary = IsTemporary,
                    InstanceId = InstanceId,
                    ConfigureConsumeTopology = ConfigureConsumeTopology,
                });
        }

    }

}
