namespace Cogito.MassTransit.Azure.ServiceBus.Registration
{

    public class ServiceBusBusOptions
    {

        public string ConnectionString { get; set; }

        public bool UseDefaultCredential { get; set; } = true;

    }

}
