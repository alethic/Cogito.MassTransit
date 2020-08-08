using System;
using System.Collections.Generic;
using System.Linq;

namespace Cogito.MassTransit.Registration
{

    /// <summary>
    /// Gets all of the known <see cref="BusDefinition"/> instances.
    /// </summary>
    public class BusDefinitionProvider
    {

        readonly IEnumerable<IBusDefinitionSource> sources;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="sources"></param>
        public BusDefinitionProvider(IEnumerable<IBusDefinitionSource> sources)
        {
            this.sources = sources ?? throw new ArgumentNullException(nameof(sources));
        }

        /// <summary>
        /// Gets all of the known <see cref="BusDefinition"/> instances.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<BusDefinition> GetDefinitions() => sources.SelectMany(i => i.GetDefinitions());

    }

}
