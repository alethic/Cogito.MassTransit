using System.Collections.Generic;

namespace Cogito.MassTransit.Registration
{

    /// <summary>
    /// Provides configuration for a given receive endpoint.
    /// </summary>
    public interface IReceiveEndpointConfigurationSource
    {

        /// <summary>
        /// Gets the configurations to apply to the endpoint of the given name.
        /// </summary>
        /// <param name="busName"></param>
        /// <param name="endpointName"></param>
        /// <returns></returns>
        IEnumerable<IReceiveEndpointConfiguration> GetConfiguration(string busName, string endpointName);

    }

}