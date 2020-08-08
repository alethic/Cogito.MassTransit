using System.Collections.Generic;

namespace Cogito.MassTransit.Registration
{

    /// <summary>
    /// Provides registered <see cref="SagaDefinition"/> instances.
    /// </summary>
    public interface ISagaDefinitionSource
    {

        /// <summary>
        /// Obtains the registered <see cref="SagaDefinition"/> instances.
        /// </summary>
        /// <returns></returns>
        IEnumerable<SagaDefinition> GetDefinitions();

    }

}
