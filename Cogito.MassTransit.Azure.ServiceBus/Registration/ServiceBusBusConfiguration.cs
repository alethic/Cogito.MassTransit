﻿
using System;

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

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="busName"></param>
        /// <param name="options"></param>
        public ServiceBusBusConfiguration(IOptionsSnapshot<ServiceBusBusOptions> options)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
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
            c.Host(o.ConnectionString);
        }
    }

}
