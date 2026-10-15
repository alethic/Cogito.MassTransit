using System;
using System.Linq;
using System.Threading.Tasks;

using Cogito.DependencyInjection;
using Cogito.MassTransit.DependencyInjection;

using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using MassTransit.EntityFrameworkCoreIntegration.Saga;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Xunit;

namespace Cogito.MassTransit.EntityFrameworkCore.Tests
{

    /// <summary>
    /// Registers this assembly by scanning it, as an application would. Every type here that carries a registration
    /// attribute must register cleanly; cases that should fail apply the attribute to an unattributed type instead.
    /// </summary>
    public class EntityFrameworkSagaRepositoryAttributeTests
    {

        public class OptimisticStart
        {

            public Guid Id { get; set; }

        }

        public class OptimisticSaga : SagaStateMachineInstance
        {

            public Guid CorrelationId { get; set; }

            public string? CurrentState { get; set; }

        }

        [AddSagaStateMachine(EndpointName = "ef-optimistic")]
        [EntityFrameworkSagaRepository(typeof(SagaDbContext), ConcurrencyMode = ConcurrencyMode.Optimistic)]
        public class OptimisticStateMachine : MassTransitStateMachine<OptimisticSaga>
        {

            public OptimisticStateMachine()
            {
                InstanceState(x => x.CurrentState);
                Event(() => Started, x => x.CorrelateById(m => m.Message.Id));
                Initially(When(Started).TransitionTo(Running));
            }

            public State Running { get; private set; } = null!;

            public Event<OptimisticStart> Started { get; private set; } = null!;

        }

        public class PessimisticStart
        {

            public Guid Id { get; set; }

        }

        public class PessimisticSaga : SagaStateMachineInstance
        {

            public Guid CorrelationId { get; set; }

            public string? CurrentState { get; set; }

        }

        [AddSagaStateMachine(EndpointName = "ef-pessimistic")]
        [EntityFrameworkSagaRepository(typeof(SagaDbContext), Database = SagaRepositoryDatabase.Sqlite)]
        public class PessimisticStateMachine : MassTransitStateMachine<PessimisticSaga>
        {

            public PessimisticStateMachine()
            {
                InstanceState(x => x.CurrentState);
                Event(() => Started, x => x.CorrelateById(m => m.Message.Id));
                Initially(When(Started).TransitionTo(Running));
            }

            public State Running { get; private set; } = null!;

            public Event<PessimisticStart> Started { get; private set; } = null!;

        }

        /// <summary>
        /// Unattributed, so attribute misuse can be tried on it without breaking the scan.
        /// </summary>
        public class UnattributedStateMachine : MassTransitStateMachine<OptimisticSaga>
        {

        }

        public class SagaDbContext : DbContext
        {

            public SagaDbContext(DbContextOptions<SagaDbContext> options) :
                base(options)
            {

            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<OptimisticSaga>().HasKey(x => x.CorrelationId);
                modelBuilder.Entity<PessimisticSaga>().HasKey(x => x.CorrelationId);
            }

        }

        static async Task<ServiceProvider> StartAsync(SqliteConnection connection)
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDbContext<SagaDbContext>(o => o.UseSqlite(connection));
            services.AddMassTransit(x => x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context)));
            services.AddFromAttributes(typeof(EntityFrameworkSagaRepositoryAttributeTests).Assembly);

            var provider = services.BuildServiceProvider(true);

            using (var scope = provider.CreateScope())
                await scope.ServiceProvider.GetRequiredService<SagaDbContext>().Database.EnsureCreatedAsync();

            foreach (var service in provider.GetServices<IHostedService>())
                await service.StartAsync(default);

            return provider;
        }

        static async Task StopAsync(ServiceProvider provider)
        {
            foreach (var service in provider.GetServices<IHostedService>().Reverse())
                await service.StopAsync(default);

            await provider.DisposeAsync();
        }

        static async Task<string?> WaitForStateAsync<TSaga>(ServiceProvider provider, Guid id, Func<TSaga, string?> state)
            where TSaga : class, SagaStateMachineInstance
        {
            for (var i = 0; i < 100; i++)
            {
                using (var scope = provider.CreateScope())
                {
                    var saga = await scope.ServiceProvider.GetRequiredService<SagaDbContext>().Set<TSaga>().AsNoTracking().SingleOrDefaultAsync(x => x.CorrelationId == id);
                    if (saga != null)
                        return state(saga);
                }

                await Task.Delay(100);
            }

            throw new TimeoutException($"Saga {id} was not saved.");
        }

        [Fact]
        public async Task Optimistic_saga_is_saved_in_the_db_context()
        {
            using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();

            var provider = await StartAsync(connection);

            try
            {
                Assert.IsType<OptimisticSagaRepositoryLockStrategy<OptimisticSaga>>(provider.GetRequiredService<ISagaRepositoryLockStrategy<OptimisticSaga>>());

                var id = Guid.NewGuid();
                var endpoint = await provider.GetRequiredService<IBus>().GetSendEndpoint(new Uri("queue:ef-optimistic"));
                await endpoint.Send(new OptimisticStart() { Id = id });

                Assert.Equal("Running", await WaitForStateAsync<OptimisticSaga>(provider, id, x => x.CurrentState));
            }
            finally
            {
                await StopAsync(provider);
            }
        }

        [Fact]
        public async Task Pessimistic_saga_is_saved_in_the_db_context()
        {
            using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();

            var provider = await StartAsync(connection);

            try
            {
                Assert.IsType<PessimisticSagaRepositoryLockStrategy<PessimisticSaga>>(provider.GetRequiredService<ISagaRepositoryLockStrategy<PessimisticSaga>>());

                var id = Guid.NewGuid();
                var endpoint = await provider.GetRequiredService<IBus>().GetSendEndpoint(new Uri("queue:ef-pessimistic"));
                await endpoint.Send(new PessimisticStart() { Id = id });

                Assert.Equal("Running", await WaitForStateAsync<PessimisticSaga>(provider, id, x => x.CurrentState));
            }
            finally
            {
                await StopAsync(provider);
            }
        }

        [Fact]
        public void Db_context_type_must_be_a_db_context()
        {
            var attribute = new EntityFrameworkSagaRepositoryAttribute(typeof(string));
            Assert.Throws<InvalidOperationException>(() => attribute.Add(new ServiceCollection(), typeof(UnattributedStateMachine)));
        }

    }

}
