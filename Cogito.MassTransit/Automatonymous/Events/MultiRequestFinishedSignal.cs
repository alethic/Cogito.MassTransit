using Automatonymous;

namespace Cogito.MassTransit.Automatonymous.Events
{

    /// <summary>
    /// Signals to the state machine upon completion of the multi-request.
    /// </summary>
    /// <typeparam name="TInstance"></typeparam>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    public class MultiRequestFinishedSignal<TInstance, TRequest, TResponse>
        where TInstance : class, SagaStateMachineInstance
        where TRequest : class
        where TResponse : class
    {



    }

}
