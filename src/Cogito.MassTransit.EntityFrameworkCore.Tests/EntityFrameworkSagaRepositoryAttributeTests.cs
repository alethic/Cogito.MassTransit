using System;
using System.IO;
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
    /// Registers the types each test needs with <c>AddFromAttributes</c>, as an application would.
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

        [EntityFrameworkSagaRepository(typeof(string))]
        public class NotADbContextStateMachine : MassTransitStateMachine<OptimisticSaga>
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

        /// <summary>
        /// A database file of its own, so that each context opens its own connection to it, and the test's reads
        /// are isolated from the saga repository's transactions as they would be on a real database.
        /// </summary>
        sealed class TemporaryDatabase : IDisposable
        {

            readonly string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".db");

            public string ConnectionString => new SqliteConnectionStringBuilder() { DataSource = path, Pooling = false }.ToString();

            public void Dispose()
            {
                File.Delete(path);
            }

        }

        static async Task<ServiceProvider> StartAsync(TemporaryDatabase database, Type type)
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDbContext<SagaDbContext>(o => o.UseSqlite(database.ConnectionString));
            services.AddMassTransit(x => x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context)));
            services.AddFromAttributes(type);

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
            using var database = new TemporaryDatabase();

            var provider = await StartAsync(database, typeof(OptimisticStateMachine));

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
            using var database = new TemporaryDatabase();

            var provider = await StartAsync(database, typeof(PessimisticStateMachine));

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
            var services = new ServiceCollection();
            Assert.Throws<InvalidOperationException>(() => services.AddFromAttributes(typeof(NotADbContextStateMachine)));
        }

    }

}
