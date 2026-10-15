using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Cogito.DependencyInjection;
using Cogito.MassTransit.DependencyInjection;

using MassTransit;
using MassTransit.Configuration;
using MassTransit.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Xunit;

namespace Cogito.MassTransit.Tests
{

    public class DependencyInjectionAttributeTests
    {

        public interface IOtherBus : IBus
        {

        }

        public class NamedPing
        {

            public Guid Id { get; set; }

        }

        [AddConsumer(EndpointName = "di-named")]
        public class NamedConsumer : IConsumer<NamedPing>
        {

            public Task Consume(ConsumeContext<NamedPing> context)
            {
                Received.Signal(context.Message.Id, context.ReceiveContext.InputAddress);
                return Task.CompletedTask;
            }

        }

        public class OtherPing
        {

            public Guid Id { get; set; }

        }

        [AddConsumer(Bus = typeof(IOtherBus), EndpointName = "di-other")]
        public class OtherBusConsumer : IConsumer<OtherPing>
        {

            public Task Consume(ConsumeContext<OtherPing> context)
            {
                Received.Signal(context.Message.Id, context.ReceiveContext.InputAddress);
                return Task.CompletedTask;
            }

        }

        public class DefinedPing
        {

            public Guid Id { get; set; }

        }

        [AddConsumer(Definition = typeof(DefinedConsumerDefinition))]
        public class DefinedConsumer : IConsumer<DefinedPing>
        {

            public Task Consume(ConsumeContext<DefinedPing> context)
            {
                Received.Signal(context.Message.Id, context.ReceiveContext.InputAddress);
                return Task.CompletedTask;
            }

        }

        public class DefinedConsumerDefinition : ConsumerDefinition<DefinedConsumer>
        {

            public DefinedConsumerDefinition()
            {
                EndpointName = "di-defined";
            }

        }

        [AddConsumer(ExcludeFromConfigureEndpoints = true)]
        public class ExcludedConsumer : IConsumer<NamedPing>
        {

            public Task Consume(ConsumeContext<NamedPing> context) => Task.CompletedTask;

        }

        [AddConsumer(ExcludeFromConfigureEndpoints = true, EndpointName = "di-excluded")]
        public class ExcludedNamedConsumer : IConsumer<NamedPing>
        {

            public Task Consume(ConsumeContext<NamedPing> context) => Task.CompletedTask;

        }

        public class RepositorySagaStart
        {

            public Guid Id { get; set; }

        }

        public class RepositorySaga : SagaStateMachineInstance
        {

            public Guid CorrelationId { get; set; }

            public string? CurrentState { get; set; }

        }

        [AddSagaStateMachine(EndpointName = "di-saga")]
        [InMemorySagaRepository]
        public class RepositoryStateMachine : MassTransitStateMachine<RepositorySaga>
        {

            public RepositoryStateMachine()
            {
                InstanceState(x => x.CurrentState);
                Event(() => Started, x => x.CorrelateById(m => m.Message.Id));
                Initially(When(Started).Then(context => Received.Signal(context.Message.Id, context.ReceiveContext.InputAddress)).TransitionTo(Running));
            }

            public State Running { get; private set; } = null!;

            public Event<RepositorySagaStart> Started { get; private set; } = null!;

        }

        public class ProviderSagaStart
        {

            public Guid Id { get; set; }

        }

        public class ProviderSaga : SagaStateMachineInstance
        {

            public Guid CorrelationId { get; set; }

            public string? CurrentState { get; set; }

        }

        [AddSagaStateMachine]
        public class ProviderStateMachine : MassTransitStateMachine<ProviderSaga>
        {

            public ProviderStateMachine()
            {
                InstanceState(x => x.CurrentState);
                Event(() => Started, x => x.CorrelateById(m => m.Message.Id));
                Initially(When(Started).Then(context => Received.Signal(context.Message.Id, context.ReceiveContext.InputAddress)).TransitionTo(Running));
            }

            public State Running { get; private set; } = null!;

            public Event<ProviderSagaStart> Started { get; private set; } = null!;

        }

        public class ClassSagaStart : CorrelatedBy<Guid>
        {

            public Guid CorrelationId { get; set; }

        }

        [AddSaga(EndpointName = "di-class-saga")]
        [InMemorySagaRepository]
        public class ClassSaga : ISaga, InitiatedBy<ClassSagaStart>
        {

            public Guid CorrelationId { get; set; }

            public Task Consume(ConsumeContext<ClassSagaStart> context)
            {
                Received.Signal(context.Message.CorrelationId, context.ReceiveContext.InputAddress);
                return Task.CompletedTask;
            }

        }

        public class ExecuteArguments
        {

            public Guid Id { get; set; }

        }

        [AddExecuteActivity(EndpointName = "di-execute")]
        public class ExecuteOnlyActivity : IExecuteActivity<ExecuteArguments>
        {

            public Task<ExecutionResult> Execute(ExecuteContext<ExecuteArguments> context)
            {
                Received.Signal(context.Arguments.Id, context.ReceiveContext.InputAddress);
                return Task.FromResult(context.Completed());
            }

        }

        public class ActivityArguments
        {

            public Guid Id { get; set; }

        }

        public class ActivityLog
        {

            public Guid Id { get; set; }

        }

        [AddActivity(ExecuteEndpointName = "di-activity-execute", CompensateEndpointName = "di-activity-compensate")]
        public class CompensatingActivity : IActivity<ActivityArguments, ActivityLog>
        {

            public Task<ExecutionResult> Execute(ExecuteContext<ActivityArguments> context)
            {
                Received.Signal(context.Arguments.Id, context.ReceiveContext.InputAddress);
                return Task.FromResult(context.Completed(new ActivityLog() { Id = context.Arguments.Id }));
            }

            public Task<CompensationResult> Compensate(CompensateContext<ActivityLog> context)
            {
                return Task.FromResult(context.Compensated());
            }

        }

        /// <summary>
        /// Records the address each message was received on, by the message's id.
        /// </summary>
        static class Received
        {

            static readonly ConcurrentDictionary<Guid, TaskCompletionSource<Uri>> received = new();

            public static void Signal(Guid id, Uri? address)
            {
                received.GetOrAdd(id, _ => new TaskCompletionSource<Uri>(TaskCreationOptions.RunContinuationsAsynchronously)).TrySetResult(address!);
            }

            public static async Task<string> WaitAsync(Guid id)
            {
                var task = received.GetOrAdd(id, _ => new TaskCompletionSource<Uri>(TaskCreationOptions.RunContinuationsAsynchronously)).Task;
                if (await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(10))) != task)
                    throw new TimeoutException($"Message {id} was not received.");

                return (await task).AbsolutePath.Trim('/');
            }

        }

        static async Task<ServiceProvider> StartAsync(Action<IServiceCollection> configure)
        {
            var services = new ServiceCollection();
            services.AddLogging();
            configure(services);

            var provider = services.BuildServiceProvider(true);

            try
            {
                foreach (var service in provider.GetServices<IHostedService>())
                    await service.StartAsync(default);
            }
            catch
            {
                await StopAsync(provider);
                throw;
            }

            return provider;
        }

        static async Task StopAsync(ServiceProvider provider)
        {
            foreach (var service in provider.GetServices<IHostedService>().Reverse())
                await service.StopAsync(default);

            await provider.DisposeAsync();
        }

        static void AddBus(IServiceCollection services, Action<IBusRegistrationConfigurator>? configure = null)
        {
            services.AddMassTransit(x =>
            {
                configure?.Invoke(x);
                x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
            });
        }

        static async Task SendAsync(IBus bus, string queue, object message)
        {
            var endpoint = await bus.GetSendEndpoint(new Uri("queue:" + queue));
            await endpoint.Send(message);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Consumer_receives_on_its_named_endpoint_whether_added_before_or_after_the_bus(bool before)
        {
            var provider = await StartAsync(services =>
            {
                if (before)
                    services.AddFromAttributes(typeof(NamedConsumer));

                AddBus(services);

                if (before == false)
                    services.AddFromAttributes(typeof(NamedConsumer));
            });

            try
            {
                var id = Guid.NewGuid();
                await SendAsync(provider.GetRequiredService<IBus>(), "di-named", new NamedPing() { Id = id });
                Assert.Equal("di-named", await Received.WaitAsync(id));
            }
            finally
            {
                await StopAsync(provider);
            }
        }

        [Fact]
        public async Task Consumer_is_registered_onto_the_selected_bus_only()
        {
            var provider = await StartAsync(services =>
            {
                AddBus(services);
                services.AddMassTransit<IOtherBus>(x => x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context)));
                services.AddFromAttributes(typeof(OtherBusConsumer));
            });

            try
            {
                Assert.DoesNotContain(provider.GetServices<IConsumerRegistration>(), i => i.Type == typeof(OtherBusConsumer));
                Assert.Contains(provider.GetServices<Bind<IOtherBus, IConsumerRegistration>>(), i => i.Value.Type == typeof(OtherBusConsumer));

                var id = Guid.NewGuid();
                await SendAsync(provider.GetRequiredService<IOtherBus>(), "di-other", new OtherPing() { Id = id });
                Assert.Equal("di-other", await Received.WaitAsync(id));
            }
            finally
            {
                await StopAsync(provider);
            }
        }

        [Fact]
        public async Task Consumer_uses_its_definition()
        {
            var provider = await StartAsync(services =>
            {
                AddBus(services);
                services.AddFromAttributes(typeof(DefinedConsumer));
            });

            try
            {
                var id = Guid.NewGuid();
                await SendAsync(provider.GetRequiredService<IBus>(), "di-defined", new DefinedPing() { Id = id });
                Assert.Equal("di-defined", await Received.WaitAsync(id));
            }
            finally
            {
                await StopAsync(provider);
            }
        }

        [Fact]
        public void Consumer_can_be_excluded_from_configure_endpoints()
        {
            var services = new ServiceCollection();
            services.AddFromAttributes(typeof(ExcludedConsumer));

            using var provider = services.BuildServiceProvider();
            Assert.False(provider.GetServices<IConsumerRegistration>().Single(i => i.Type == typeof(ExcludedConsumer)).IncludeInConfigureEndpoints);
        }

        [Fact]
        public void Excluded_consumer_cannot_configure_its_endpoint()
        {
            var services = new ServiceCollection();
            Assert.Throws<InvalidOperationException>(() => services.AddFromAttributes(typeof(ExcludedNamedConsumer)));
        }

        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(IDisposable))]
        public void Bus_must_be_an_interface_extending_IBus(Type bus)
        {
            Assert.Throws<ArgumentException>(() => new AddConsumerAttribute() { Bus = bus });
        }

        [Fact]
        public async Task Saga_state_machine_with_a_repository_attribute_receives_on_its_named_endpoint()
        {
            var provider = await StartAsync(services =>
            {
                AddBus(services);
                services.AddFromAttributes(typeof(RepositoryStateMachine));
            });

            try
            {
                var id = Guid.NewGuid();
                await SendAsync(provider.GetRequiredService<IBus>(), "di-saga", new RepositorySagaStart() { Id = id });
                Assert.Equal("di-saga", await Received.WaitAsync(id));
            }
            finally
            {
                await StopAsync(provider);
            }
        }

        [Fact]
        public async Task Saga_with_a_repository_attribute_receives_on_its_named_endpoint()
        {
            var provider = await StartAsync(services =>
            {
                AddBus(services);
                services.AddFromAttributes(typeof(ClassSaga));
            });

            try
            {
                var id = Guid.NewGuid();
                await SendAsync(provider.GetRequiredService<IBus>(), "di-class-saga", new ClassSagaStart() { CorrelationId = id });
                Assert.Equal("di-class-saga", await Received.WaitAsync(id));
            }
            finally
            {
                await StopAsync(provider);
            }
        }

        [Fact]
        public async Task Saga_without_a_repository_fails_startup()
        {
            var e = await Assert.ThrowsAsync<InvalidOperationException>(() => StartAsync(services =>
            {
                AddBus(services, x => x.SetInMemorySagaRepositoryProvider());
                services.AddFromAttributes(typeof(ProviderStateMachine));
            }));

            Assert.Contains(typeof(ProviderSaga).FullName!, e.Message);
        }

        [Fact]
        public async Task Saga_without_a_repository_attribute_uses_the_bus_provider_when_added_before_the_bus()
        {
            var provider = await StartAsync(services =>
            {
                services.AddFromAttributes(typeof(ProviderStateMachine));
                AddBus(services, x => x.SetInMemorySagaRepositoryProvider());
            });

            try
            {
                var id = Guid.NewGuid();
                await provider.GetRequiredService<IBus>().Publish(new ProviderSagaStart() { Id = id });
                await Received.WaitAsync(id);
            }
            finally
            {
                await StopAsync(provider);
            }
        }

        [Fact]
        public async Task Execute_activity_executes_on_its_named_endpoint()
        {
            var provider = await StartAsync(services =>
            {
                AddBus(services);
                services.AddFromAttributes(typeof(ExecuteOnlyActivity));
            });

            try
            {
                var id = Guid.NewGuid();
                var builder = new RoutingSlipBuilder(NewId.NextGuid());
                builder.AddActivity("Execute", new Uri("queue:di-execute"), new { Id = id });
                await provider.GetRequiredService<IBus>().Execute(builder.Build());
                Assert.Equal("di-execute", await Received.WaitAsync(id));
            }
            finally
            {
                await StopAsync(provider);
            }
        }

        [Fact]
        public async Task Activity_executes_and_compensates_on_its_named_endpoints()
        {
            var provider = await StartAsync(services =>
            {
                AddBus(services);
                services.AddFromAttributes(typeof(CompensatingActivity));
            });

            try
            {
                var formatter = DefaultEndpointNameFormatter.Instance;
                Assert.Equal("di-activity-execute", provider.GetRequiredService<IEndpointDefinition<IExecuteActivity<ActivityArguments>>>().GetEndpointName(formatter));
                Assert.Equal("di-activity-compensate", provider.GetRequiredService<IEndpointDefinition<ICompensateActivity<ActivityLog>>>().GetEndpointName(formatter));

                var id = Guid.NewGuid();
                var builder = new RoutingSlipBuilder(NewId.NextGuid());
                builder.AddActivity("Compensating", new Uri("queue:di-activity-execute"), new { Id = id });
                await provider.GetRequiredService<IBus>().Execute(builder.Build());
                Assert.Equal("di-activity-execute", await Received.WaitAsync(id));
            }
            finally
            {
                await StopAsync(provider);
            }
        }

    }

}
