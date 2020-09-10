using System;
using System.Collections.Generic;

namespace Cogito.MassTransit.Registration
{

    /// <summary>
    /// Provides a <see cref="ISagaStateMachineDefinitionSource"/> based on registered instances.
    /// </summary>
    public class ContainerSagaStateMachineDefinitionSource : ISagaStateMachineDefinitionSource
    {

        readonly IEnumerable<SagaStateMachineDefinition> definitions;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="definitions"></param>
        public ContainerSagaStateMachineDefinitionSource(IEnumerable<SagaStateMachineDefinition> definitions)
        {
            this.definitions = definitions ?? throw new ArgumentNullException(nameof(definitions));
        }

        /// <summary>
        /// Gets the available <see cref="SagaStateMachineDefinition"/> instances.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SagaStateMachineDefinition> GetDefinitions() => definitions;

    }

}
