using MassTransit;

namespace Cogito.MassTransit.Registration
{

    /// <summary>
    /// Describes a class which can apply configuration to a bus.
    /// </summary>
    public interface IBusConfiguration
    {

        /// <summary>
        /// Configures the factory.
        /// </summary>
        /// <param name="busName"></param>
        /// <param name="configurator"></param>
        void Apply(string busName, IBusFactoryConfigurator configurator);

    }

}