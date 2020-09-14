using System;
using System.Collections.Generic;
using System.Linq;

namespace Cogito.MassTransit.Registration
{

    /// <summary>
    /// Provides names from registered consumer definitions.
    /// </summary>
    public class BusDefinitionNameSource : IBusNameSource
    {

        readonly BusDefinitionProvider definitions;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="definitions"></param>
        public BusDefinitionNameSource(BusDefinitionProvider definitions)
        {
            this.definitions = definitions ?? throw new ArgumentNullException(nameof(definitions));
        }

        public IEnumerable<string> GetBusNames()
        {
            return definitions.GetDefinitions().Select(i => i.BusName);
        }

    }

}
