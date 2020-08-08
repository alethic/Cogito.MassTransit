using System;
using System.Collections.Generic;
using System.Linq;

namespace Cogito.MassTransit.Registration
{

    /// <summary>
    /// Provides names from registered consumer definitions.
    /// </summary>
    public class ConsumerDefinitionNameSource : IBusNameSource, IReceiveEndpointNameSource
    {

        readonly ConsumerDefinitionProvider definitions;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="definitions"></param>
        public ConsumerDefinitionNameSource(ConsumerDefinitionProvider definitions)
        {
            this.definitions = definitions ?? throw new ArgumentNullException(nameof(definitions));
        }

        /// <summary>
        /// Gets the known bus names.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetBusNames()
        {
            return definitions.GetDefinitions().Select(i => i.BusName);
        }

        /// <summary>
        /// Gets the known endpoint names for the specified bus name.
        /// </summary>
        /// <param name="busName"></param>
        /// <returns></returns>
        public IEnumerable<string> GetEndpointNames(string busName)
        {
            return definitions.GetDefinitions().Where(i => i.BusName == busName).Select(i => i.EndpointName);
        }

    }

}
