using System;
using System.Data;

using Cogito.MassTransit.DependencyInjection;

using MassTransit;
using MassTransit.Configuration;
using MassTransit.DependencyInjection.Registration;
using MassTransit.EntityFrameworkCoreIntegration;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Cogito.MassTransit.EntityFrameworkCore.DependencyInjection
{

    /// <summary>
    /// Keeps the saga's instances in Entity Framework Core, through a <see cref="DbContext"/> registered in the
    /// container. Apply it to the saga, or to the saga state machine.
    /// </summary>
    public class EntityFrameworkSagaRepositoryAttribute : SagaRepositoryAttribute
    {

        ConcurrencyMode? concurrencyMode;
        IsolationLevel? isolationLevel;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="dbContextType">The <see cref="DbContext"/> that maps the saga, which must be registered in the container.</param>
        public EntityFrameworkSagaRepositoryAttribute(Type dbContextType)
        {
            DbContextType = dbContextType ?? throw new ArgumentNullException(nameof(dbContextType));
        }

        /// <summary>
        /// The <see cref="DbContext"/> that maps the saga, which must be registered in the container.
        /// </summary>
        public Type DbContextType { get; }

        /// <summary>
        /// Whether instances are locked when loaded, or checked for changes when saved. Unless set, they are locked.
        /// </summary>
        public ConcurrencyMode ConcurrencyMode
        {
            get => concurrencyMode ?? ConcurrencyMode.Pessimistic;
            set => concurrencyMode = value;
        }

        /// <summary>
        /// The isolation level of the transaction instances are loaded and saved in. Unless set, it is
        /// <see cref="IsolationLevel.Serializable"/> when instances are locked, and <see cref="IsolationLevel.ReadCommitted"/>
        /// when they are checked for changes.
        /// </summary>
        public IsolationLevel IsolationLevel
        {
            get => isolationLevel ?? (ConcurrencyMode == ConcurrencyMode.Optimistic ? IsolationLevel.ReadCommitted : IsolationLevel.Serializable);
            set => isolationLevel = value;
        }

        /// <summary>
        /// The database, whose statement locks instances when they are loaded. Unless set, it is SQL Server.
        /// </summary>
        public SagaRepositoryDatabase Database { get; set; } = SagaRepositoryDatabase.SqlServer;

        /// <summary>
        /// The schema the saga's table is in, when it is not the <see cref="DbContext"/>'s default.
        /// </summary>
        public string? Schema { get; set; }

        /// <inheritdoc />
        protected override void AddSagaRepository(IServiceCollection services, Type sagaType)
        {
            if (typeof(DbContext).IsAssignableFrom(DbContextType) == false)
                throw new InvalidOperationException($"{DbContextType} is not a DbContext.");

            ((IRegister)Activator.CreateInstance(typeof(Register<,>).MakeGenericType(sagaType, DbContextType))!).AddSagaRepository(services, this);
        }

        interface IRegister
        {

            void AddSagaRepository(IServiceCollection services, EntityFrameworkSagaRepositoryAttribute attribute);

        }

        class Register<TSaga, TDbContext> : IRegister
            where TSaga : class, ISaga
            where TDbContext : DbContext
        {

            public void AddSagaRepository(IServiceCollection services, EntityFrameworkSagaRepositoryAttribute attribute)
            {
                var configurator = new EntityFrameworkSagaRepositoryConfigurator<TSaga>();
                configurator.ExistingDbContext<TDbContext>();
                configurator.ConcurrencyMode = attribute.ConcurrencyMode;
                configurator.IsolationLevel = attribute.IsolationLevel;
                configurator.LockStatementProvider = CreateLockStatementProvider(attribute.Database, attribute.Schema);
                configurator.Validate().ThrowIfContainsFailure("The Entity Framework saga repository configuration is invalid:");
                configurator.Register(new SagaRepositoryRegistrationConfigurator<TSaga>(services));
            }

        }

        static ILockStatementProvider CreateLockStatementProvider(SagaRepositoryDatabase database, string? schema) => database switch
        {
            SagaRepositoryDatabase.SqlServer => schema != null ? new SqlServerLockStatementProvider(schema) : new SqlServerLockStatementProvider(),
            SagaRepositoryDatabase.Postgres => schema != null ? new PostgresLockStatementProvider(schema) : new PostgresLockStatementProvider(),
            SagaRepositoryDatabase.MySql => new MySqlLockStatementProvider(),
            SagaRepositoryDatabase.Sqlite => schema != null ? new SqliteLockStatementProvider(schema) : new SqliteLockStatementProvider(),
            SagaRepositoryDatabase.Oracle => schema != null ? new OracleLockStatementProvider(schema) : new OracleLockStatementProvider(),
            _ => throw new InvalidOperationException($"Unknown saga repository database {database}."),
        };

    }

}
