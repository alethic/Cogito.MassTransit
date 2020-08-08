using System;
using System.Collections.Generic;

namespace Cogito.MassTransit.Registration
{

    /// <summary>
    /// Provides a <see cref="ISagaDefinitionSource"/> based on registered instances.
    /// </summary>
    public class ContainerSagaDefinitionSource : ISagaDefinitionSource
    {

        readonly IEnumerable<SagaDefinition> definitions;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="definitions"></param>
        public ContainerSagaDefinitionSource(IEnumerable<SagaDefinition> definitions)
        {
            this.definitions = definitions ?? throw new ArgumentNullException(nameof(definitions));
        }

        /// <summary>
        /// Gets the available <see cref="SagaDefinition"/> instances.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SagaDefinition> GetDefinitions() => definitions;

    }

}
