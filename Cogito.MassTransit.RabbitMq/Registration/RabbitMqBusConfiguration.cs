using System;

using Cogito.MassTransit.Registration;

using MassTransit;
using MassTransit.RabbitMqTransport;

using Microsoft.Extensions.Options;

namespace Cogito.MassTransit.RabbitMq.Registration
{

    /// <summary>
    /// Applies named configuration to a RabbitMQ configurator.
    /// </summary>
    public class RabbitMqBusConfiguration : IBusConfiguration
    {

        readonly IOptionsSnapshot<RabbitMqBusOptions> options;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="busName"></param>
        /// <param name="options"></param>
        public RabbitMqBusConfiguration(IOptionsSnapshot<RabbitMqBusOptions> options)
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
            var c = configurator as IRabbitMqBusFactoryConfigurator;
            if (c == null)
                return;

            var o = options.Get(busName);
            if (o == null)
                return;

            // generate MT host
            c.Host(o.Host, o.Port, o.VirtualHost, o.ConnectionName, s =>
            {
                if (o.UserName != null)
                    s.Username(o.UserName);
                if (o.Password != null)
                    s.Password(o.UserName);
                if (o.EnableSsl)
                    s.UseSsl(_ => { });
            });
        }

    }

}
