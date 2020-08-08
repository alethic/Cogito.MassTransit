namespace Cogito.MassTransit.Registration
{

    /// <summary>
    /// Metadata attached to a class which indicates which receive endpoint it will be applicable to.
    /// </summary>
    public interface IReceiveEndpointMetadata
    {

        /// <summary>
        /// Name of the bus to which the class is applicable.
        /// </summary>
        string BusName { get; }

        /// <summary>
        /// Name of the receive endpoint to which the class is applicable.
        /// </summary>
        string EndpointName { get; }

    }

}
