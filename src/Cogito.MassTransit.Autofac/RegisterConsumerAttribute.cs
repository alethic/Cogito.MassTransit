using System;

using Cogito.Autofac;
using Cogito.MassTransit.Registration;

using MassTransit;

namespace Cogito.MassTransit.Autofac
{

    /// <summary>
    /// Registers the class as a <see cref="IConsumer"/> to consume messages on a given receive endpoint.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class RegisterConsumerAttribute : Attribute, IRegistrationRootAttribute, IReceiveEndpointMetadata
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="endpointName"></param>
        public RegisterConsumerAttribute()
        {

        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="endpointName"></param>
        public RegisterConsumerAttribute(string endpointName) :
            this("", endpointName)
        {

        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="endpointName"></param>
        public RegisterConsumerAttribute(string busName, string endpointName)
        {
            BusName = busName ?? throw new ArgumentNullException(nameof(busName));
            EndpointName = endpointName ?? throw new ArgumentNullException(nameof(endpointName));
        }

        /// <summary>
        /// Name of the bus this type applies to.
        /// </summary>
        public string BusName { get; set; }

        /// <summary>
        /// Name of the endpoint this type applies to.
        /// </summary>
        public string EndpointName { get; set; }

        Type IRegistrationRootAttribute.HandlerType => typeof(RegisterConsumerHandler);

    }

}
