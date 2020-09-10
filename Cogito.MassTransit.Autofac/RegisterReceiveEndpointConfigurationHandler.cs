using System;
using System.Collections.Generic;
using System.Linq;

using Autofac;
using Autofac.Integration.Mef;

using Cogito.Autofac;
using Cogito.MassTransit.Registration;

namespace Cogito.MassTransit.Autofac
{

    class RegisterReceiveEndpointConfigurationHandler : IRegistrationHandler
    {

        public void Register(ContainerBuilder builder, Type type, IEnumerable<IRegistrationRootAttribute> attributes)
        {
            builder.RegisterMetadataRegistrationSources();

            foreach (var attr in attributes.OfType<IReceiveEndpointMetadata>())
                builder.RegisterReceiveEndpointConfiguration(type, attr.BusName ?? "", attr.EndpointName);
        }

    }

}
