using System;
using System.Collections.Generic;
using System.Linq;

using Autofac;

using Cogito.Autofac;
using Cogito.MassTransit.Registration;

namespace Cogito.MassTransit.Autofac
{

    class RegisterReceiveEndpointConfigurationHandler : IRegistrationHandler
    {

        public void Register(ContainerBuilder builder, Type type, IEnumerable<IRegistrationRootAttribute> attributes)
        {
            builder.RegisterModule<AssemblyModule>();

            foreach (var attr in attributes.OfType<IReceiveEndpointMetadata>())
                builder.RegisterReceiveEndpointConfiguration(type, attr.BusName ?? "", attr.EndpointName);
        }

    }

}
