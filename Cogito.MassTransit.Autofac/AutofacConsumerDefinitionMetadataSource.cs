using System;
using System.Collections.Generic;
using System.Linq;

using Autofac;
using Autofac.Core;

using Cogito.Autofac;
using Cogito.Collections;
using Cogito.MassTransit.Registration;

using MassTransit;
using MassTransit.Internals.Extensions;
using MassTransit.Saga;

namespace Cogito.MassTransit.Autofac
{

    /// <summary>
    /// Provides <see cref="ConsumerDefinition"/> instances based on the registered consumer types and metadata.
    /// </summary>
    [RegisterAs(typeof(IConsumerDefinitionSource))]
    public class AutofacConsumerDefinitionMetadataSource : IConsumerDefinitionSource
    {

        readonly IComponentContext context;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="context"></param>
        public AutofacConsumerDefinitionMetadataSource(IComponentContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Obtains <see cref="ConsumerDefinition"/> instances based on metadata.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ConsumerDefinition> GetDefinitions()
        {
            return context.ComponentRegistry.Registrations
                .SelectMany(r => r.Services.OfType<IServiceWithType>(), (r, s) => new { r, s })
                .Where(rs => rs.s.ServiceType.HasInterface<IConsumer>())
                .Select(i => new { i.s, bs = (string)i.r.Metadata.GetOrDefault("BusName"), ep = (string)i.r.Metadata.GetOrDefault("EndpointName") })
                .Where(i => i.bs != null && i.ep != null && !i.s.ServiceType.HasInterface<ISaga>())
                .Select(i => new ConsumerDefinition(i.s.ServiceType, i.bs, i.ep));
        }

    }

}
