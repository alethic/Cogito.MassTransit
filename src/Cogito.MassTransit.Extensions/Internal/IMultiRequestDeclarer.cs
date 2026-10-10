using System;

using MassTransit;

namespace Cogito.MassTransit.Extensions.Internal
{

    /// <summary>
    /// The operations needed to declare a multi-request on a state machine. Implemented once over the protected
    /// members of <see cref="MassTransitStateMachine{TSaga}"/>, and once over <see cref="IStateMachineModifier{TSaga}"/>.
    /// </summary>
    /// <typeparam name="TSaga"></typeparam>
    interface IMultiRequestDeclarer<TSaga>
        where TSaga : class, SagaStateMachineInstance
    {

        /// <summary>
        /// Declares an event with the specified name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        Event<T> Event<T>(string name, Action<IEventCorrelationConfigurator<TSaga, T>>? configure = null)
            where T : class;

        /// <summary>
        /// Declares a state with the specified name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        State State(string name);

        /// <summary>
        /// Binds activities to the event, in any state, when the filter accepts it.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="event"></param>
        /// <param name="filter"></param>
        /// <param name="configure"></param>
        void DuringAny<T>(Event<T> @event, StateMachineCondition<TSaga, T> filter, Func<EventActivityBinder<TSaga, T>, EventActivityBinder<TSaga, T>> configure)
            where T : class;

    }

}
