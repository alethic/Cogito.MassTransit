using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using Autofac;

using Cogito.MassTransit.Registration;

namespace Cogito.MassTransit.Autofac
{

    /// <summary>
    /// Loads sagas from the container onto the receive endpoint.
    /// </summary>
    public class SagaStateMachineDefinitionReceiveEndpointConfigurationSource : IReceiveEndpointConfigurationSource
    {

        readonly SagaStateMachineDefinitionProvider definitions;
        readonly IComponentContext context;
        readonly ConcurrentDictionary<SagaStateMachineDefinition, SagaStateMachineDefinitionReceiveEndpointConfiguration> cache = new ConcurrentDictionary<SagaStateMachineDefinition, SagaStateMachineDefinitionReceiveEndpointConfiguration>();

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="definitions"></param>
        /// <param name="context"></param>
        public SagaStateMachineDefinitionReceiveEndpointConfigurationSource(SagaStateMachineDefinitionProvider definitions, IComponentContext context)
        {
            this.definitions = definitions ?? throw new ArgumentNullException(nameof(definitions));
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Loads the configurations to apply sagas to the endpoint.
        /// </summary>
        /// <param name="busName"></param>
        /// <param name="endpointName"></param>
        /// <returns></returns>
        public IEnumerable<IReceiveEndpointConfiguration> GetConfiguration(string busName, string endpointName)
        {
            return definitions.GetDefinitions().Where(i => i.BusName == busName && i.EndpointName == endpointName).Select(i => cache.GetOrAdd(i, _ => new SagaStateMachineDefinitionReceiveEndpointConfiguration(_, context)));
        }
    }

}
