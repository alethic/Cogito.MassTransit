using System.Collections.Generic;

namespace Cogito.MassTransit.Registration
{

    /// <summary>
    /// Provides registered <see cref="SagaStateMachineDefinition"/> instances.
    /// </summary>
    public interface ISagaStateMachineDefinitionSource
    {

        /// <summary>
        /// Obtains the registered <see cref="SagaStateMachineDefinition"/> instances.
        /// </summary>
        /// <returns></returns>
        IEnumerable<SagaStateMachineDefinition> GetDefinitions();

    }

}
