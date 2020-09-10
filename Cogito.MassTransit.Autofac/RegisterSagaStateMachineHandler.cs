using System;
using System.Collections.Generic;
using System.Linq;

using Autofac;

using Cogito.Autofac;
using Cogito.MassTransit.Registration;

namespace Cogito.MassTransit.Autofac
{

    class RegisterSagaStateMachineHandler : IRegistrationHandler
    {

        public void Register(ContainerBuilder builder, Type type, IEnumerable<IRegistrationRootAttribute> attributes)
        {
            foreach (var attr in attributes.OfType<IReceiveEndpointMetadata>())
                builder.RegisterSagaStateMachine(type, attr.BusName ?? "", attr.EndpointName);
        }

    }

}
