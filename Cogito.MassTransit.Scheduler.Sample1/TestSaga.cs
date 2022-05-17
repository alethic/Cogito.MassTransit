using System.Threading;
using System.Threading.Tasks;

using Automatonymous;

using Cogito.Autofac;
using Cogito.MassTransit.Autofac;

using MassTransit;

using Microsoft.Extensions.Hosting;

namespace Cogito.MassTransit.Scheduler.Sample1
{

    [RegisterSagaStateMachine("foo")]
    public class TestSaga : MassTransitStateMachine<TestSagaInstance>
    {




    }

}
