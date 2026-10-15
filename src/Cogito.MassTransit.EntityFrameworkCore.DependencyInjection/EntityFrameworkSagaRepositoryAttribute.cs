using System;
using System.Data;
using System.Linq;

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
        /// The <see cref="ILockStatementProvider"/> whose statement locks instances when they are loaded, such as
        /// <see cref="PostgresLockStatementProvider"/>. It must have a constructor whose parameters are all optional.
        /// Unless set, MassTransit's default is used, which is for SQL Server.
        /// </summary>
        public Type? LockStatementProvider { get; set; }

        /// <inheritdoc />
        protected override void AddSagaRepository(IServiceCollection services, Type sagaType)
        {
            if (typeof(DbContext).IsAssignableFrom(DbContextType) == false)
                throw new InvalidOperationException($"{DbContextType} is not a DbContext.");
            if (LockStatementProvider != null && (LockStatementProvider.IsAbstract || typeof(ILockStatementProvider).IsAssignableFrom(LockStatementProvider) == false))
                throw new InvalidOperationException($"{LockStatementProvider} is not an ILockStatementProvider that can be created.");

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
                if (attribute.LockStatementProvider != null)
                    configurator.LockStatementProvider = CreateLockStatementProvider(attribute.LockStatementProvider);
                configurator.Validate().ThrowIfContainsFailure("The Entity Framework saga repository configuration is invalid:");
                configurator.Register(new SagaRepositoryRegistrationConfigurator<TSaga>(services));
            }

        }

        /// <summary>
        /// Creates the lock statement provider with a constructor whose parameters are all optional, taking their
        /// defaults, as MassTransit's providers have.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        static ILockStatementProvider CreateLockStatementProvider(Type type)
        {
            var constructor = type.GetConstructors()
                .Where(i => i.GetParameters().All(j => j.IsOptional))
                .OrderBy(i => i.GetParameters().Length)
                .FirstOrDefault();
            if (constructor == null)
                throw new InvalidOperationException($"{type} has no constructor whose parameters are all optional.");

            return (ILockStatementProvider)constructor.Invoke(constructor.GetParameters().Select(i => i.DefaultValue).ToArray());
        }

    }

}
