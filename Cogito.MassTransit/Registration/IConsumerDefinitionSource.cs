using System.Collections.Generic;

namespace Cogito.MassTransit.Registration
{

    /// <summary>
    /// Provides registered <see cref="ConsumerDefinition"/> instances.
    /// </summary>
    public interface IConsumerDefinitionSource
    {

        /// <summary>
        /// Obtains the registered <see cref="ConsumerDefinition"/> instances.
        /// </summary>
        /// <returns></returns>
        IEnumerable<ConsumerDefinition> GetDefinitions();

    }

}
