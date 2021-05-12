namespace Cogito.MassTransit.RabbitMq.Registration
{

    public class RabbitMqBusOptions
    {

        public string Host { get; set; } = "localhost";

        public ushort Port { get; set; } = 5672;

        public string VirtualHost { get; set; }

        public string UserName { get; set; } = "guest";

        public string Password { get; set; } = "guest";

        public string ConnectionName { get; set; } = "accutraq";

        public bool EnableSsl { get; set; } = true;

    }

}
