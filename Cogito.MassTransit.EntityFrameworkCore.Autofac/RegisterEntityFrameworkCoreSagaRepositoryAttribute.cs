using System;
using System.Data;

using Cogito.Autofac;

using MassTransit.EntityFrameworkCoreIntegration;

using Microsoft.EntityFrameworkCore;

namespace Cogito.MassTransit.EntityFrameworkCore.Autofac
{

    /// <summary>
    /// Registers the Entity Framework Core saga repository for the decorated state machine.
    /// </summary>
    public class RegisterEntityFrameworkCoreSagaRepositoryAttribute : Attribute, IRegistrationRootAttribute
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public RegisterEntityFrameworkCoreSagaRepositoryAttribute(Type dbContextType)
        {
            DbContextType = dbContextType;
        }

        /// <summary>
        /// Gets or sets the <see cref="DbContext"/> type which is resolved for the state machine instance.
        /// </summary>
        public Type DbContextType { get; set; }

        /// <summary>
        /// Gets or sets the concurrency mode of the state machine.
        /// </summary>
        public ConcurrencyMode ConcurrencyMode { get; set; }

        /// <summary>
        /// Gets or sets the isolation level of the state machine.
        /// </summary>
        public IsolationLevel IsolationLevel { get; set; } = IsolationLevel.ReadCommitted;

        /// <summary>
        /// Optionally the name of the correlation ID columns to use for pessimistic locks.
        /// </summary>
        public string CorrelationIdColumnName { get; set; }

        Type IRegistrationRootAttribute.HandlerType => typeof(RegisterEntityFrameworkCoreSagaRepositoryHandler);

    }

}
