using System;

using Cogito.MassTransit.Registration;

using MassTransit;

namespace Cogito.MassTransit.Autofac.Sample1
{

    [RegisterReceiveEndpointConfiguration("other")]
    public class OtherReceiveEndpointConfiguration : IReceiveEndpointConfiguration
    {

        public void Apply(string busName, string endpointName, IReceiveEndpointConfigurator configurator)
        {

        }

    }

}
