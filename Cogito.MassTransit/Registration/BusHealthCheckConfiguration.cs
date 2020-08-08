using MassTransit;
using MassTransit.Monitoring.Health;

namespace Cogito.MassTransit.Registration
{

    /// <summary>
    /// Applies the Bus healthcheck configuration.
    /// </summary>
    public class BusHealthCheckConfiguration : IBusConfiguration
    {

        readonly BusHealth busHealth;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="busHealth"></param>
        public BusHealthCheckConfiguration(BusHealth busHealth)
        {
            this.busHealth = busHealth ?? throw new System.ArgumentNullException(nameof(busHealth));
        }

        public void Apply(string busName, IBusFactoryConfigurator configurator)
        {
            configurator.ConnectBusObserver(busHealth);
            configurator.ConnectEndpointConfigurationObserver(busHealth);
        }

    }

}
