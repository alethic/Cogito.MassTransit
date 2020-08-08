using System;
using System.Collections.Generic;

namespace Cogito.MassTransit.Registration
{

    /// <summary>
    /// Provides a <see cref="IConsumerDefinitionSource"/> based on registered instances.
    /// </summary>
    public class ContainerConsumerDefinitionSource : IConsumerDefinitionSource
    {

        readonly IEnumerable<ConsumerDefinition> definitions;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="definitions"></param>
        public ContainerConsumerDefinitionSource(IEnumerable<ConsumerDefinition> definitions)
        {
            this.definitions = definitions ?? throw new ArgumentNullException(nameof(definitions));
        }

        /// <summary>
        /// Gets the available <see cref="ConsumerDefinition"/> instances.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ConsumerDefinition> GetDefinitions() => definitions;

    }

}
