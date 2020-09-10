using System;
using System.Collections.Generic;
using System.Linq;

using Cogito.Autofac;
using Cogito.MassTransit.Registration;

namespace Cogito.MassTransit.Autofac
{

    [RegisterAs(typeof(IReceiveEndpointConfigurationSource))]
    [RegisterAs(typeof(IReceiveEndpointNameSource))]
    [RegisterAs(typeof(IBusNameSource))]
    [RegisterWithAttributeFiltering]
    public class AutofacReceiveEndpointConfigurationMetadataSource : IReceiveEndpointConfigurationSource, IReceiveEndpointNameSource, IBusNameSource
    {

        readonly IEnumerable<Lazy<IReceiveEndpointConfiguration, IReceiveEndpointMetadata>> configurations;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="configurations"></param>
        public AutofacReceiveEndpointConfigurationMetadataSource(IEnumerable<Lazy<IReceiveEndpointConfiguration, IReceiveEndpointMetadata>> configurations)
        {
            this.configurations = configurations ?? throw new ArgumentNullException(nameof(configurations));
        }

        public IEnumerable<string> GetBusNames()
        {
            return configurations.Select(i => i.Metadata.BusName).Distinct();
        }

        public IEnumerable<string> GetEndpointNames(string busName)
        {
            return configurations.Where(i => i.Metadata.BusName == busName).Select(i => i.Metadata.EndpointName).Distinct();
        }

        public IEnumerable<IReceiveEndpointConfiguration> GetConfiguration(string busName, string endpointName)
        {
            return configurations.Where(i => i.Metadata.BusName == busName && i.Metadata.EndpointName == endpointName).Select(i => i.Value);
        }
    }

}
