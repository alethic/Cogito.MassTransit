using System;
using System.Collections.Generic;
using System.Linq;

using Autofac;
using Autofac.Core;

using Automatonymous;

using Cogito.Autofac;
using Cogito.Collections;
using Cogito.MassTransit.Registration;

using MassTransit.Internals.Extensions;

namespace Cogito.MassTransit.Autofac
{

    /// <summary>
    /// Provides <see cref="SagaStateMachineDefinition"/> instances based on the registered saga types and metadata.
    /// </summary>
    [RegisterAs(typeof(ISagaStateMachineDefinitionSource))]
    [RegisterInstancePerLifetimeScope]
    public class AutofacSagaStateMachineDefinitionMetadataSource : ISagaStateMachineDefinitionSource
    {

        readonly IComponentContext context;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="context"></param>
        public AutofacSagaStateMachineDefinitionMetadataSource(IComponentContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Obtains <see cref="SagaStateMachineDefinition"/> instances based on metadata.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SagaStateMachineDefinition> GetDefinitions()
        {
            return context.ComponentRegistry.Registrations
                .SelectMany(r => r.Services.OfType<IServiceWithType>(), (r, s) => new { r, s })
                .Where(rs => rs.s.ServiceType.HasInterface<StateMachine>())
                .Where(rs => rs.s.ServiceType.IsGenericType && rs.s.ServiceType.GetGenericTypeDefinition() == typeof(SagaStateMachine<>))
                .Where(rs => rs.s.ServiceType.GetClosingArgument(typeof(SagaStateMachine<>)) != null)
                .Select(i => new { i.s, bs = (string)i.r.Metadata.GetOrDefault("BusName"), ep = (string)i.r.Metadata.GetOrDefault("EndpointName") })
                .Where(i => i.bs != null && i.ep != null)
                .Select(i => new SagaStateMachineDefinition(i.s.ServiceType, i.bs, i.ep));
        }

    }

}
