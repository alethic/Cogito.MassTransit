using Autofac;

using Cogito.Autofac;

using MassTransit;

namespace Cogito.MassTransit.Autofac.Sample1
{

    public class AssemblyModule : ModuleBase
    {

        protected override void Register(ContainerBuilder builder)
        {
            builder.RegisterFromAttributes(typeof(AssemblyModule).Assembly);
            builder.RegisterInMemorySagaRepository<TestSagaState>();
        }

    }

}
