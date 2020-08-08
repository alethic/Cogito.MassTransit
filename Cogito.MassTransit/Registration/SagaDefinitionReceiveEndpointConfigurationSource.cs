using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Cogito.MassTransit.Registration
{

    /// <summary>
    /// Loads sagas from the container onto the receive endpoint.
    /// </summary>
    public class SagaDefinitionReceiveEndpointConfigurationSource : IReceiveEndpointConfigurationSource
    {

        readonly SagaDefinitionProvider definitions;
        readonly ConcurrentDictionary<SagaDefinition, SagaDefinitionReceiveEndpointConfiguration> cache = new ConcurrentDictionary<SagaDefinition, SagaDefinitionReceiveEndpointConfiguration>();

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="definitions"></param>
        public SagaDefinitionReceiveEndpointConfigurationSource(SagaDefinitionProvider definitions)
        {
            this.definitions = definitions ?? throw new ArgumentNullException(nameof(definitions));
        }

        /// <summary>
        /// Loads the configurations to apply sagas to the endpoint.
        /// </summary>
        /// <param name="busName"></param>
        /// <param name="endpointName"></param>
        /// <returns></returns>
        public IEnumerable<IReceiveEndpointConfiguration> GetConfiguration(string busName, string endpointName)
        {
            return definitions.GetDefinitions().Where(i => i.BusName == busName && i.EndpointName == endpointName).Select(i => cache.GetOrAdd(i, _ => new SagaDefinitionReceiveEndpointConfiguration(_)));
        }
    }

}
