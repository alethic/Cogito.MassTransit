using System;

using Cogito.MassTransit.Registration.Internal;

using MassTransit;
using MassTransit.Scoping;

namespace Cogito.MassTransit.Registration
{

    /// <summary>
    /// Applies a consumer to an endpoint.
    /// </summary>
    public class ConsumerDefinitionReceiveEndpointConfiguration : IReceiveEndpointConfiguration
    {

        readonly ConsumerDefinition definition;
        readonly IConsumerScopeProvider scopeProvider;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="definition"></param>
        public ConsumerDefinitionReceiveEndpointConfiguration(ConsumerDefinition definition, IConsumerScopeProvider scopeProvider)
        {
            this.definition = definition ?? throw new ArgumentNullException(nameof(definition));
            this.scopeProvider = scopeProvider ?? throw new ArgumentNullException(nameof(scopeProvider));
        }

        /// <summary>
        /// Applies the consumer to the endpoint.
        /// </summary>
        /// <param name="busName"></param>
        /// <param name="endpointName"></param>
        /// <param name="configurator"></param>
        public void Apply(string busName, string endpointName, IReceiveEndpointConfigurator configurator)
        {
            ConsumerConfiguratorCache.Configure(definition.Type, configurator, scopeProvider);
        }

    }

}
