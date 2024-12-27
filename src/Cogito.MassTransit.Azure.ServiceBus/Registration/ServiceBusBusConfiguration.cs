
using System;

using Cogito.Azure.Identity;
using Cogito.MassTransit.Registration;

using MassTransit;
using MassTransit.Azure.ServiceBus.Core;

using Microsoft.Extensions.Options;

namespace Cogito.MassTransit.Azure.ServiceBus.Registration
{

    /// <summary>
    /// Applies named configuration to a Serivce Bus configurator.
    /// </summary>
    public class ServiceBusBusConfiguration : IBusConfiguration
    {

        readonly IOptionsSnapshot<ServiceBusBusOptions> options;
        readonly AzureIdentityCredential credential;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="busName"></param>
        /// <param name="options"></param>
        public ServiceBusBusConfiguration(IOptionsSnapshot<ServiceBusBusOptions> options, AzureIdentityCredential credential)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.credential = credential ?? throw new ArgumentNullException(nameof(credential));
        }

        /// <summary>
        /// Applies the configuration for the given bus to the configurator.
        /// </summary>
        /// <param name="busName"></param>
        /// <param name="configurator"></param>
        public void Apply(string busName, IBusFactoryConfigurator configurator)
        {
            var c = configurator as IServiceBusBusFactoryConfigurator;
            if (c == null)
                return;

            var o = options.Get(busName);
            if (o == null)
                return;

            // apply connection string
            if (o.ConnectionString != null)
                c.Host(o.ConnectionString, c => ConfigureServiceBusHost(c, o));
        }

        /// <summary>
        /// Applies additional configuration to the service bus.
        /// </summary>
        /// <param name="configurator"></param>
        /// <param name="options"></param>
        void ConfigureServiceBusHost(IServiceBusHostConfigurator configurator, ServiceBusBusOptions options)
        {

        }

    }

}
