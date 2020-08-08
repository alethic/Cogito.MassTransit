using System;
using System.Collections.Generic;
using System.Linq;

namespace Cogito.MassTransit.Registration
{

    /// <summary>
    /// Provides names from registered saga definitions.
    /// </summary>
    public class SagaDefinitionNameSource : IBusNameSource, IReceiveEndpointNameSource
    {

        readonly SagaDefinitionProvider definitions;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="definitions"></param>
        public SagaDefinitionNameSource(SagaDefinitionProvider definitions)
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
