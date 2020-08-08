using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using MassTransit.Scoping;

namespace Cogito.MassTransit.Registration
{

    /// <summary>
    /// Loads consumers from Autofac onto the receive endpoint.
    /// </summary>
    public class ConsumerDefinitionReceiveEndpointConfigurationSource : IReceiveEndpointConfigurationSource
    {

        readonly ConsumerDefinitionProvider definitions;
        readonly IConsumerScopeProvider scopeProvider;
        readonly ConcurrentDictionary<ConsumerDefinition, ConsumerDefinitionReceiveEndpointConfiguration> cache = new ConcurrentDictionary<ConsumerDefinition, ConsumerDefinitionReceiveEndpointConfiguration>();

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="definitions"></param>
        /// <param name="scopeProvider"></param>
        public ConsumerDefinitionReceiveEndpointConfigurationSource(ConsumerDefinitionProvider definitions, IConsumerScopeProvider scopeProvider)
        {
            this.definitions = definitions ?? throw new ArgumentNullException(nameof(definitions));
            this.scopeProvider = scopeProvider ?? throw new ArgumentNullException(nameof(scopeProvider));
        }

        /// <summary>
        /// Loads the configurations to apply consumers to the endpoint.
        /// </summary>
        /// <param name="busName"></param>
        /// <param name="endpointName"></param>
        /// <returns></returns>
        public IEnumerable<IReceiveEndpointConfiguration> GetConfiguration(string busName, string endpointName)
        {
            return definitions.GetDefinitions().Where(i => i.BusName == busName && i.EndpointName == endpointName).Select(i => cache.GetOrAdd(i, _ => new ConsumerDefinitionReceiveEndpointConfiguration(_, scopeProvider)));
        }
    }

}
