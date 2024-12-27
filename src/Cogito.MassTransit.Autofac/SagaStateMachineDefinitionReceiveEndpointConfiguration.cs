using System;

using Autofac;

using Cogito.MassTransit.Autofac.Internal;
using Cogito.MassTransit.Registration;

using MassTransit;

namespace Cogito.MassTransit.Autofac
{

    /// <summary>
    /// Applies a saga state machine to an endpoint.
    /// </summary>
    public class SagaStateMachineDefinitionReceiveEndpointConfiguration : IReceiveEndpointConfiguration
    {

        readonly SagaStateMachineDefinition definition;
        readonly IComponentContext context;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="definition"></param>
        /// <param name="context"></param>
        public SagaStateMachineDefinitionReceiveEndpointConfiguration(SagaStateMachineDefinition definition, IComponentContext context)
        {
            this.definition = definition ?? throw new ArgumentNullException(nameof(definition));
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Applies the saga state machine to the endpoint.
        /// </summary>
        /// <param name="busName"></param>
        /// <param name="endpointName"></param>
        /// <param name="configurator"></param>
        public void Apply(string busName, string endpointName, IReceiveEndpointConfigurator configurator)
        {
            SagaStateMachineConfiguratorCache.Configure(definition.Type, configurator, context);
        }

    }

}
