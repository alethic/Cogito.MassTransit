using System;

using MassTransit;

namespace Cogito.MassTransit.Registration
{

    /// <summary>
    /// Applies a saga to an endpoint.
    /// </summary>
    public class SagaDefinitionReceiveEndpointConfiguration : IReceiveEndpointConfiguration
    {

        readonly SagaDefinition definition;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="definition"></param>
        public SagaDefinitionReceiveEndpointConfiguration(SagaDefinition definition)
        {
            this.definition = definition ?? throw new ArgumentNullException(nameof(definition));
        }

        /// <summary>
        /// Applies the consumer to the endpoint.
        /// </summary>
        /// <param name="busName"></param>
        /// <param name="endpointName"></param>
        /// <param name="configurator"></param>
        public void Apply(string busName, string endpointName, IReceiveEndpointConfigurator configurator)
        {
            throw new NotImplementedException();
        }

    }

}
