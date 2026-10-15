using System;

using MassTransit;

using Microsoft.Extensions.DependencyInjection;

namespace Cogito.MassTransit.DependencyInjection
{

    /// <summary>
    /// Keeps the saga's instances in memory, where they are lost when the process ends.
    /// </summary>
    public class InMemorySagaRepositoryAttribute : SagaRepositoryAttribute
    {

        /// <inheritdoc />
        protected override void AddSagaRepository(IServiceCollection services, Type sagaType)
        {
            ((IRegister)Activator.CreateInstance(typeof(Register<>).MakeGenericType(sagaType))!).AddSagaRepository(services);
        }

        interface IRegister
        {

            void AddSagaRepository(IServiceCollection services);

        }

        class Register<TSaga> : IRegister
            where TSaga : class, ISaga
        {

            public void AddSagaRepository(IServiceCollection services)
            {
                services.RegisterInMemorySagaRepository<TSaga>();
            }

        }

    }

}
