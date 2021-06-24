using Autofac;
using Autofac.Integration.Mef;

using Cogito.Autofac;
using Cogito.MassTransit.Autofac.Internal;
using Cogito.MassTransit.Registration;

using MassTransit;
using MassTransit.AutofacIntegration;
using MassTransit.AutofacIntegration.Registration;
using MassTransit.AutofacIntegration.ScopeProviders;
using MassTransit.Context;
using MassTransit.Registration;
using MassTransit.Scoping;
using MassTransit.Transactions;
using MassTransit.Transports;

namespace Cogito.MassTransit.Autofac
{

    public class AssemblyModule : ModuleBase
    {

        /// <summary>
        /// Registers any additional scoped classes.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="context"></param>
        static void RegisterScope(ContainerBuilder builder, ConsumeContext context)
        {
            builder.RegisterGeneric(typeof(ConsumeContextRequestClientProxy<>)).AsImplementedInterfaces();
        }

        /// <summary>
        /// Creates the <see cref="IConsumerScopeProvider"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        static IConsumerScopeProvider CreateConsumerScopeProvider(IComponentContext context)
        {
            var lifetimeScopeProvider = new SingleLifetimeScopeProvider(context.Resolve<ILifetimeScope>());
            return new AutofacConsumerScopeProvider(lifetimeScopeProvider, "message", RegisterScope);
        }

        /// <summary>
        /// Creates the <see cref="ISendEndpointProvider"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        static ISendEndpointProvider GetCurrentSendEndpointProvider(IComponentContext context)
        {
            if (context.TryResolve(out ConsumeContext consumeContext))
                return consumeContext;

            var bus = context.ResolveOptional<ITransactionalBus>() ?? context.Resolve<IBus>();
            return new ScopedSendEndpointProvider<ILifetimeScope>(bus, context.Resolve<ILifetimeScope>());
        }

        /// <summary>
        /// Creates the <see cref="IPublishEndpoint"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        static IPublishEndpoint GetCurrentPublishEndpoint(IComponentContext context)
        {
            if (context.TryResolve(out ConsumeContext consumeContext))
                return consumeContext;

            var bus = context.ResolveOptional<ITransactionalBus>() ?? context.Resolve<IBus>();
            return new PublishEndpoint(new ScopedPublishEndpointProvider<ILifetimeScope>(bus, context.Resolve<ILifetimeScope>()));
        }

        protected override void Register(ContainerBuilder builder)
        {
            builder.RegisterMetadataRegistrationSources();
            builder.RegisterModule<Cogito.Autofac.AssemblyModule>();
            builder.RegisterFromAttributes(typeof(AssemblyModule).Assembly);

            // setup global logging
            builder.RegisterBuildCallback(i => LogContext.ConfigureCurrentLogContext(i.Resolve<Microsoft.Extensions.Logging.ILoggerFactory>()));

            // standard Autofac related services
            builder.RegisterType<BusDepot>().As<IBusDepot>().SingleInstance();
            builder.Register(CreateConsumerScopeProvider).As<IConsumerScopeProvider>().SingleInstance().IfNotRegistered(typeof(IConsumerScopeProvider));
            builder.Register(GetCurrentSendEndpointProvider).As<ISendEndpointProvider>().InstancePerLifetimeScope();
            builder.Register(GetCurrentPublishEndpoint).As<IPublishEndpoint>().InstancePerLifetimeScope();
            builder.Register(context => new AutofacConfigurationServiceProvider(context.Resolve<ILifetimeScope>())).As<IConfigurationServiceProvider>().SingleInstance().IfNotRegistered(typeof(IConfigurationServiceProvider));

            // consumer related services
            builder.RegisterType<ConsumerDefinitionProvider>().SingleInstance();
            builder.RegisterType<ConsumerDefinitionReceiveEndpointConfigurationSource>().As<IReceiveEndpointConfigurationSource>().SingleInstance();
            builder.RegisterType<ConsumerDefinitionNameSource>().As<IBusNameSource>().As<IReceiveEndpointNameSource>().SingleInstance();
            builder.RegisterType<ContainerConsumerDefinitionSource>().As<IConsumerDefinitionSource>().SingleInstance();

            // saga related services
            builder.RegisterType<SagaDefinitionProvider>().SingleInstance();
            builder.RegisterType<SagaDefinitionReceiveEndpointConfigurationSource>().As<IReceiveEndpointConfigurationSource>().SingleInstance();
            builder.RegisterType<SagaDefinitionNameSource>().As<IBusNameSource>().As<IReceiveEndpointNameSource>().SingleInstance();
            builder.RegisterType<ContainerSagaDefinitionSource>().As<ISagaDefinitionSource>().SingleInstance();

            // saga state machine related services
            builder.RegisterType<SagaStateMachineDefinitionProvider>().SingleInstance();
            builder.RegisterType<SagaStateMachineDefinitionReceiveEndpointConfigurationSource>().As<IReceiveEndpointConfigurationSource>().SingleInstance();
            builder.RegisterType<SagaStateMachineDefinitionNameSource>().As<IBusNameSource>().As<IReceiveEndpointNameSource>().SingleInstance();
            builder.RegisterType<ContainerSagaStateMachineDefinitionSource>().As<ISagaStateMachineDefinitionSource>().SingleInstance();

            // bus related services
            builder.RegisterType<ContainerBusConfigurationSource>().As<IBusConfigurationSource>().SingleInstance();
            builder.RegisterType<ContainerBusObserverConfiguration>().As<IBusConfiguration>().SingleInstance();
            builder.RegisterType<ContainerBusReceiveEndpointConfiguration>().As<IBusConfiguration>().SingleInstance();
            builder.RegisterType<ContainerBusDefinitionSource>().As<IBusDefinitionSource>().SingleInstance();

            // providers
            builder.RegisterType<BusDefinitionNameSource>().As<IBusNameSource>().SingleInstance();
            builder.RegisterType<BusDefinitionProvider>().SingleInstance();
            builder.RegisterType<BusNameProvider>().SingleInstance();
            builder.RegisterType<BusConfigurationProvider>().SingleInstance();
            builder.RegisterType<ReceiveEndpointNameProvider>().SingleInstance();
            builder.RegisterType<ReceiveEndpointConfigurationProvider>().SingleInstance();
            builder.RegisterType<BusRegistrationFactory>().SingleInstance();
            builder.RegisterType<BusProvider>().SingleInstance();

            // generates request clients
            builder.RegisterGenericRequestClient();

            // register default bus instance
            builder.Register(ctx => ctx.Resolve<BusProvider>().GetBus()).SingleInstance().ExternallyOwned().As<IBusControl>().As<IBus>();
            builder.Register(ctx => ctx.Resolve<IBus>().CreateClientFactory()).SingleInstance();
        }

    }

}
