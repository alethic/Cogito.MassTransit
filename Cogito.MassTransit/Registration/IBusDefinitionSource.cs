using System.Collections.Generic;

namespace Cogito.MassTransit.Registration
{

    /// <summary>
    /// Provides definitions of buses.
    /// </summary>
    public interface IBusDefinitionSource
    {

        /// <summary>
        /// Gets the known bus definitions.
        /// </summary>
        /// <returns></returns>
        IEnumerable<BusDefinition> GetDefinitions();

    }

}
