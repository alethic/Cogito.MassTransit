﻿using System;

namespace Cogito.MassTransit.Registration
{

    /// <summary>
    /// Describes a registered saga.
    /// </summary>
    public class SagaDefinition : IReceiveEndpointMetadata
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="endpointName"></param>
        public SagaDefinition(Type type, string busName, string endpointName)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            BusName = busName ?? throw new ArgumentNullException(nameof(busName));
            EndpointName = endpointName ?? throw new ArgumentNullException(nameof(endpointName));
        }

        /// <summary>
        /// Gets the type of the saga.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Gets the name of the bus of the saga.
        /// </summary>
        public string BusName { get; }

        /// <summary>
        /// Name of the saga.
        /// </summary>
        public string EndpointName { get; }

    }

}
