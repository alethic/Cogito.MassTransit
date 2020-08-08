using System;
using System.Collections.Generic;
using System.Linq;

namespace Cogito.MassTransit.Registration
{

    /// <summary>
    /// Gets all of the known <see cref="ConsumerDefinition"/> instances.
    /// </summary>
    public class ConsumerDefinitionProvider
    {

        readonly IEnumerable<IConsumerDefinitionSource> sources;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="sources"></param>
        public ConsumerDefinitionProvider(IEnumerable<IConsumerDefinitionSource> sources)
        {
            this.sources = sources ?? throw new ArgumentNullException(nameof(sources));
        }

        /// <summary>
        /// Gets all of the known <see cref="ConsumerDefinition"/> instances.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ConsumerDefinition> GetDefinitions() => sources.SelectMany(i => i.GetDefinitions());

    }

}
