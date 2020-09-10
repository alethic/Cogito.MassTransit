using System;
using System.Collections.Generic;
using System.Linq;

namespace Cogito.MassTransit.Registration
{

    /// <summary>
    /// Provides names from registered saga state machine definitions.
    /// </summary>
    public class SagaStateMachineDefinitionNameSource : IBusNameSource, IReceiveEndpointNameSource
    {

        readonly SagaStateMachineDefinitionProvider definitions;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="definitions"></param>
        public SagaStateMachineDefinitionNameSource(SagaStateMachineDefinitionProvider definitions)
        {
            this.definitions = definitions ?? throw new ArgumentNullException(nameof(definitions));
        }

        public IEnumerable<string> GetBusNames()
        {
            return definitions.GetDefinitions().Select(i => i.BusName);
        }

        public IEnumerable<string> GetEndpointNames(string busName)
        {
            return definitions.GetDefinitions().Where(i => i.BusName == busName).Select(i => i.EndpointName);
        }
    }

}
