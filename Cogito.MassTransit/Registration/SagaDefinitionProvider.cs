using System;
using System.Collections.Generic;
using System.Linq;

namespace Cogito.MassTransit.Registration
{

    /// <summary>
    /// Gets all of the known <see cref="SagaDefinition"/> instances.
    /// </summary>
    public class SagaDefinitionProvider
    {

        readonly IEnumerable<ISagaDefinitionSource> sources;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="sources"></param>
        public SagaDefinitionProvider(IEnumerable<ISagaDefinitionSource> sources)
        {
            this.sources = sources ?? throw new ArgumentNullException(nameof(sources));
        }

        /// <summary>
        /// Gets all of the known <see cref="SagaDefinition"/> instances.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SagaDefinition> GetDefinitions() => sources.SelectMany(i => i.GetDefinitions());

    }

}
