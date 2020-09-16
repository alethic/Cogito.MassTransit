using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

using Autofac;

using Automatonymous;

using Cogito.Autofac;
using Cogito.Collections;

using MassTransit.AutofacIntegration.Registration;
using MassTransit.EntityFrameworkCoreIntegration;
using MassTransit.EntityFrameworkCoreIntegration.Saga;
using MassTransit.EntityFrameworkCoreIntegration.Saga.Context;
using MassTransit.Internals.Extensions;
using MassTransit.Registration;
using MassTransit.Saga;

using Microsoft.EntityFrameworkCore;

namespace Cogito.MassTransit.EntityFrameworkCore.Autofac
{

    class RegisterEntityFrameworkCoreSagaRepositoryHandler : IRegistrationHandler
    {

        public void Register(ContainerBuilder builder, Type type, IEnumerable<IRegistrationRootAttribute> attributes)
        {
            builder.RegisterModule<AssemblyModule>();

            var attribute = attributes.OfType<RegisterEntityFrameworkCoreSagaRepositoryAttribute>().FirstOrDefault();
            if (attribute == null)
                return;

            var sagaInstanceType = type.GetClosingArgument(typeof(SagaStateMachine<>));
            if (sagaInstanceType == null)
                throw new InvalidOperationException("Cannot resolve state machine instance type.");

            var dbContextType = attribute.DbContextType;
            if (dbContextType == null)
                throw new InvalidOperationException("Missing DbContext type.");

            typeof(RegisterEntityFrameworkCoreSagaRepositoryHandler).GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                .First(i => i.Name == nameof(RegisterSagaRepository) && i.GetGenericArguments().Length == 2)
                .MakeGenericMethod(sagaInstanceType, dbContextType)
                .Invoke(null, new object[] { builder, attribute.ConcurrencyMode, attribute.IsolationLevel, attribute.CorrelationIdColumnName });
        }

        static void RegisterSagaRepository<TSaga, TDbContext>(ContainerBuilder builder, ConcurrencyMode concurrencyMode, IsolationLevel isolationLevel, string correlationIdColumnName)
            where TSaga : class, SagaStateMachineInstance
            where TDbContext : DbContext
        {
            switch (concurrencyMode)
            {
                case ConcurrencyMode.Pessimistic:
                    RegisterPessimisticSagaRepository<TSaga, TDbContext>(builder, isolationLevel, correlationIdColumnName);
                    break;
                case ConcurrencyMode.Optimistic:
                    RegisterOptimisticSagaRepository<TSaga, TDbContext>(builder, isolationLevel, correlationIdColumnName);
                    break;
                default:
                    throw new InvalidOperationException();
            }

            // register saga
            var registrar = (IContainerRegistrar)builder.Properties.GetOrAdd(typeof(AutofacContainerRegistrar).FullName, _ => new AutofacContainerRegistrar(builder));
            registrar.RegisterSagaRepository<TSaga, DbContext, SagaConsumeContextFactory<DbContext, TSaga>, EntityFrameworkSagaRepositoryContextFactory<TSaga>>();
        }

        static void RegisterOptimisticSagaRepository<TSaga, TDbContext>(ContainerBuilder builder, IsolationLevel isolationLevel, string correlationIdColumnName)
            where TSaga : class, SagaStateMachineInstance
            where TDbContext : DbContext
        {
            ILoadQueryProvider<TSaga> QueryProviderFactory(IComponentContext context)
            {
                var p = (ILoadQueryProvider<TSaga>)new DefaultSagaLoadQueryProvider<TSaga>();
                var c = context.Resolve<IOrderedEnumerable<IEntityFrameworkCoreQueryCustomizer<TSaga>>>().ToList();
                if (c.Count > 0)
                {
                    // start with first customizer
                    Func<IQueryable<TSaga>, IQueryable<TSaga>> f = c[0].Apply;

                    // layer on each additional customer
                    for (int i = 1; i < c.Count; i++)
                        f = q => c[i].Apply(f(q));

                    // wrap in custom provider
                    p = new CustomSagaLoadQueryProvider<TSaga>(p, f);
                }

                return p;
            };

            // register required types for saga repository
            builder.RegisterType<ContainerSagaDbContextFactory<TDbContext, TSaga>>().As<ISagaDbContextFactory<TSaga>>();
            builder.Register(QueryProviderFactory).As<ILoadQueryProvider<TSaga>>();
            builder.RegisterType<OptimisticLoadQueryExecutor<TSaga>>().As<ILoadQueryExecutor<TSaga>>();
            builder.RegisterType<OptimisticSagaRepositoryLockStrategy<TSaga>>().WithParameter(TypedParameter.From(isolationLevel)).As<ISagaRepositoryLockStrategy<TSaga>>();
        }

        static void RegisterPessimisticSagaRepository<TSaga, TDbContext>(ContainerBuilder builder, IsolationLevel isolationLevel, string correlationIdColumnName)
            where TSaga : class, SagaStateMachineInstance
            where TDbContext : DbContext
        {
            // optionally register a lock statement provider if column name is provided
            if (correlationIdColumnName != null)
                builder.RegisterType<ColumnSqlServerLockStatementProvider<TSaga>>().WithParameter("columnName", correlationIdColumnName);

            // register required types for saga repository
            builder.RegisterType<ContainerSagaDbContextFactory<TDbContext, TSaga>>().As<ISagaDbContextFactory<TSaga>>();
            builder.RegisterType<DefaultSagaLoadQueryProvider<TSaga>>().As<ILoadQueryProvider<TSaga>>();
            builder.Register(ctx => new PessimisticLoadQueryExecutor<TSaga>((ILockStatementProvider)ctx.ResolveOptional<ColumnSqlServerLockStatementProvider<TSaga>>() ?? new SqlServerLockStatementProvider(), q => ctx.ResolveOptional<IEntityFrameworkCoreQueryCustomizer<TSaga>>()?.Apply(q))).As<ILoadQueryExecutor<TSaga>>();
            builder.RegisterType<PessimisticSagaRepositoryLockStrategy<TSaga>>().WithParameter(TypedParameter.From(isolationLevel)).As<ISagaRepositoryLockStrategy<TSaga>>();
        }

    }

}