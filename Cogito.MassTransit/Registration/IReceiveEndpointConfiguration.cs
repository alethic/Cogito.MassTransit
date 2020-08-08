using MassTransit;

namespace Cogito.MassTransit.Registration
{

    /// <summary>
    /// Describes a class which can apply configuration to a receive endpoint.
    /// </summary>
    public interface IReceiveEndpointConfiguration
    {

        /// <summary>
        /// Configures the endpoint.
        /// </summary>
        /// <param name="busName"></param>
        /// <param name="endpointName"></param>
        /// <param name="configurator"></param>
        void Apply(string busName, string endpointName, IReceiveEndpointConfigurator configurator);

    }

}