using System;

using MassTransit.Configuration;

using Microsoft.Extensions.DependencyInjection;

namespace Cogito.MassTransit.DependencyInjection
{

    /// <summary>
    /// Registers the consumer with MassTransit. Apply it more than once to register the consumer onto more than
    /// one bus.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class AddConsumerAttribute : EndpointRegistrationAttribute
    {

        /// <summary>
        /// A consumer definition for the consumer, which closes <c>IConsumerDefinition&lt;T&gt;</c>.
        /// </summary>
        public Type? Definition { get; set; }

        /// <inheritdoc />
        protected override void Add(IServiceCollection services, IContainerRegistrar registrar, Type implementationType)
        {
            var registration = services.RegisterConsumer(registrar, implementationType, Definition);
            ApplyEndpoint(services, registrar, registration, typeof(ConsumerEndpointDefinition<>).MakeGenericType(implementationType), implementationType, implementationType);
        }

    }

}
