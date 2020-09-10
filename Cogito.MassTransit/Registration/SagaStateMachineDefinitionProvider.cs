using System;
using System.Collections.Generic;
using System.Linq;

namespace Cogito.MassTransit.Registration
{

    /// <summary>
    /// Gets all of the known <see cref="SagaStateMachineDefinition"/> instances.
    /// </summary>
    public class SagaStateMachineDefinitionProvider
    {

        readonly IEnumerable<ISagaStateMachineDefinitionSource> sources;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="sources"></param>
        public SagaStateMachineDefinitionProvider(IEnumerable<ISagaStateMachineDefinitionSource> sources)
        {
            this.sources = sources ?? throw new ArgumentNullException(nameof(sources));
        }

        /// <summary>
        /// Gets all of the known <see cref="SagaStateMachineDefinition"/> instances.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SagaStateMachineDefinition> GetDefinitions() => sources.SelectMany(i => i.GetDefinitions());

    }

}
