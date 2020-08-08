using System;
using System.Collections.Generic;
using System.Linq;

using Autofac;
using Autofac.Core;

using Cogito.Autofac;
using Cogito.Collections;
using Cogito.MassTransit.Registration;

using MassTransit.Internals.Extensions;
using MassTransit.Saga;

namespace Cogito.MassTransit.Autofac
{

    /// <summary>
    /// Provides <see cref="SagaDefinition"/> instances based on the registered saga types and metadata.
    /// </summary>
    [RegisterAs(typeof(ISagaDefinitionSource))]
    public class AutofacSagaDefinitionMetadataSource : ISagaDefinitionSource
    {

        readonly IComponentContext context;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="context"></param>
        public AutofacSagaDefinitionMetadataSource(IComponentContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Obtains <see cref="SagaDefinition"/> instances based on metadata.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SagaDefinition> GetDefinitions()
        {
            return context.ComponentRegistry.Registrations
                .SelectMany(r => r.Services.OfType<IServiceWithType>(), (r, s) => new { r, s })
                .Where(rs => rs.s.ServiceType.HasInterface<ISaga>())
                .Select(i => new { i.s, bs = (string)i.r.Metadata.GetOrDefault("BusName"), ep = (string)i.r.Metadata.GetOrDefault("EndpointName") })
                .Where(i => i.bs != null && i.ep != null)
                .Select(i => new SagaDefinition(i.s.ServiceType, i.bs, i.ep));
        }

    }

}
