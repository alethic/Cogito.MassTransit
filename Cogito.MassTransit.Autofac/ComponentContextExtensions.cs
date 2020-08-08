using Autofac;

using Cogito.MassTransit.Registration;

using MassTransit;

namespace Cogito.MassTransit.Autofac
{

    /// <summary>
    /// Provides extension methods to resolve MassTransit types.
    /// </summary>
    public static class ComponentContextExtensions
    {

        /// <summary>
        /// Resolves the default bus.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static IBusControl ResolveBus(this IComponentContext context) => context.Resolve<IBusControl>();

        /// <summary>
        /// Resolves a named bus.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IBusControl ResolveBus(this IComponentContext context, string name) => context.Resolve<BusProvider>().GetBus(name);

    }

}
