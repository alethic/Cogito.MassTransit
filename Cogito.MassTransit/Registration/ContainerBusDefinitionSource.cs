using System;
using System.Collections.Generic;

namespace Cogito.MassTransit.Registration
{

    /// <summary>
    /// Provides a <see cref="BusDefinition"/> based on registered instances.
    /// </summary>
    public class ContainerBusDefinitionSource : IBusDefinitionSource
    {

        readonly IEnumerable<BusDefinition> definitions;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="definitions"></param>
        public ContainerBusDefinitionSource(IEnumerable<BusDefinition> definitions)
        {
            this.definitions = definitions ?? throw new ArgumentNullException(nameof(definitions));
        }

        /// <summary>
        /// Gets the available <see cref="BusDefinition"/> instances.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<BusDefinition> GetDefinitions() => definitions;

    }

}
