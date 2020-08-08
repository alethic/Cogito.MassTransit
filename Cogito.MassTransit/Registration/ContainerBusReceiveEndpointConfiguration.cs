using System;
using System.Collections.Generic;

using Cogito.MassTransit.Registration;

using MassTransit;

namespace Cogito.MassTransit.Autofac
{

    /// <summary>
    /// Applies the receive endpoints known from the container to the bus.
    /// </summary>
    public class ContainerBusReceiveEndpointConfiguration : IBusConfiguration
    {

        readonly ReceiveEndpointNameProvider names;
        readonly ReceiveEndpointConfigurationProvider configurations;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="configuration"></param>
        public ContainerBusReceiveEndpointConfiguration(ReceiveEndpointNameProvider names, ReceiveEndpointConfigurationProvider configurations)
        {
            this.names = names ?? throw new ArgumentNullException(nameof(names));
            this.configurations = configurations ?? throw new ArgumentNullException(nameof(configurations));
        }

        /// <summary>
        /// Applies the receive endpoints to the bus.
        /// </summary>
        /// <param name="configurator"></param>
        public void Apply(string busName, IBusFactoryConfigurator configurator)
        {
            foreach (var name in names.GetEndpointNames(busName))
                configurator.ReceiveEndpoint(name, endpoint => Apply(busName, name, endpoint, configurations.GetConfigurations(busName, name)));
        }

        /// <summary>
        /// Applies each of the configurations to the endpoint.
        /// </summary>
        /// <param name="busName"></param>
        /// <param name="endpointName"></param>
        /// <param name="endpoint"></param>
        /// <param name="configurations"></param>
        void Apply(string busName, string endpointName, IReceiveEndpointConfigurator endpoint, IEnumerable<IReceiveEndpointConfiguration> configurations)
        {
            foreach (var configuration in configurations)
                configuration.Apply(busName, endpointName, endpoint);
        }

    }

}
