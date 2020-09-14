using Automatonymous;

namespace Cogito.MassTransit.Automatonymous
{

    public interface IMultiRequestConfigurator : IRequestConfigurator
    {

        bool ClearOnFinish { set; }

    }

}
