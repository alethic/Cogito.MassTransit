using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

using Autofac;
using Autofac.Extensions.DependencyInjection;

using Automatonymous;
using Cogito.Autofac;
using Cogito.Autofac.DependencyInjection;
using Cogito.MassTransit.Autofac;
using Cogito.MassTransit.EntityFrameworkCore;
using Cogito.MassTransit.EntityFrameworkCore.Autofac;
using Cogito.MassTransit.InMemory.Autofac;
using FluentAssertions;

using fm.Extensions.Logging;

using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cogito.MassTransit.EntityFrameworkCore.Tests.Autofac
{

    [TestClass]
    public class RegisterEntityFrameworkCoreSagaRepositoryHandlerTests
    {

        [RegisterAs(typeof(IEntityFrameworkCoreQueryCustomizer<TestSaga>))]
        public class TestSagaQueryCustomizer : IEntityFrameworkCoreQueryCustomizer<TestSaga>
        {

            public IQueryable<TestSaga> Apply(IQueryable<TestSaga> query)
            {
                return query;
            }

        }

        public class TestSaga : SagaStateMachineInstance
        {

            [Key]
            public Guid CorrelationId { get; set; }

            public string CurrentState { get; set; }

        }

        [RegisterSagaStateMachine("saga")]
        public class TestStateMachine : MassTransitStateMachine<TestSaga>
        {

            public TestStateMachine(ILogger<TestStateMachine> logger)
            {
                InstanceState(x => x.CurrentState);

                Event(() => TestEvent, x => x.CorrelateById(c => c.CorrelationId, c => c.Message.CorrelationId).SelectId(i => i.Message.CorrelationId));

                Initially(
                    When(TestEvent)
                        .Then(context => logger.LogDebug("hit"))
                        .Respond(context => new TestEventResponse() { CorrelationId = context.Data.CorrelationId })
                        .Finalize());
            }

            public Event<TestEvent> TestEvent { get; set; }

        }

        public class TestEvent : CorrelatedBy<Guid>
        {

            public Guid CorrelationId { get; set; }

        }

        public class TestEventResponse : CorrelatedBy<Guid>
        {

            public Guid CorrelationId { get; set; }

        }

        public class TestDbContext : DbContext
        {

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseInMemoryDatabase("Test").ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<TestSaga>();
            }

        }

        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task Should_register_optimistic_mode()
        {
            var id = NewId.NextGuid();

            var b = new ContainerBuilder();
            b.Populate(c => c.AddLogging(a => a.AddTestContext(TestContext)));
            b.RegisterMassTransitBus(c => c.UsingInMemoryBus());
            b.RegisterType<TestDbContext>().As<TestDbContext>();
            b.RegisterFromAttributes(typeof(TestStateMachine));
            b.RegisterFromAttributes(typeof(TestSagaQueryCustomizer));
            new RegisterEntityFrameworkCoreSagaRepositoryHandler().Register(b, typeof(TestStateMachine), new[] { new RegisterEntityFrameworkCoreSagaRepositoryAttribute(typeof(TestDbContext)) { ConcurrencyMode = ConcurrencyMode.Optimistic, IsolationLevel = System.Data.IsolationLevel.Unspecified } });

            var c = b.Build();
            var bus = c.Resolve<IBusControl>();
            await bus.StartAsync();

            var r = await bus.CreateRequestClient<TestEvent>(new Uri(bus.Address, "saga")).GetResponse<TestEventResponse>(new TestEvent() { CorrelationId = id });
            r.Message.CorrelationId.Should().Be(id);
        }

        [TestMethod]
        public async Task Should_register_pessimistic_mode()
        {
            var id = NewId.NextGuid();

            var b = new ContainerBuilder();
            b.Populate(c => c.AddLogging(a => a.AddTestContext(TestContext)));
            b.RegisterMassTransitBus(c => c.UsingInMemoryBus());
            b.RegisterType<TestDbContext>().As<TestDbContext>();
            b.RegisterFromAttributes(typeof(TestStateMachine));
            b.RegisterFromAttributes(typeof(TestSagaQueryCustomizer));
            new RegisterEntityFrameworkCoreSagaRepositoryHandler().Register(b, typeof(TestStateMachine), new[] { new RegisterEntityFrameworkCoreSagaRepositoryAttribute(typeof(TestDbContext)) { ConcurrencyMode = ConcurrencyMode.Pessimistic, IsolationLevel = System.Data.IsolationLevel.Unspecified } });

            var c = b.Build();
            var bus = c.Resolve<IBusControl>();
            await bus.StartAsync();

            var r = await bus.CreateRequestClient<TestEvent>(new Uri(bus.Address, "saga")).GetResponse<TestEventResponse>(new TestEvent() { CorrelationId = id });
            r.Message.CorrelationId.Should().Be(id);
        }

    }

}
