using System.Collections.Generic;

namespace Cogito.MassTransit.Registration
{

    /// <summary>
    /// Provides configuration for the MassTransit busses.
    /// </summary>
    public interface IBusConfigurationSource
    {

        /// <summary>
        /// Gets the configurations to apply to the bus factory.
        /// </summary>
        /// <returns></returns>
        IEnumerable<IBusConfiguration> Query(string busName);

    }

}
