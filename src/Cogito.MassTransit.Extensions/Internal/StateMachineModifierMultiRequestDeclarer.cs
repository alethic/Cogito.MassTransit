using System;

using MassTransit;

namespace Cogito.MassTransit.Extensions.Internal
{

    /// <summary>
    /// Declares the parts of a multi-request through an <see cref="IStateMachineModifier{TSaga}"/>.
    /// </summary>
    /// <typeparam name="TSaga"></typeparam>
    class StateMachineModifierMultiRequestDeclarer<TSaga> : IMultiRequestDeclarer<TSaga>
        where TSaga : class, SagaStateMachineInstance
    {

        readonly IStateMachineModifier<TSaga> modifier;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="modifier"></param>
        public StateMachineModifierMultiRequestDeclarer(IStateMachineModifier<TSaga> modifier)
        {
            this.modifier = modifier ?? throw new ArgumentNullException(nameof(modifier));
        }

        /// <inheritdoc/>
        public Event<T> Event<T>(string name, Action<IEventCorrelationConfigurator<TSaga, T>>? configure = null)
            where T : class
        {
            Event<T> @event;

            if (configure != null)
                modifier.Event(name, configure, out @event);
            else
                modifier.Event(name, out @event);

            return @event;
        }

        /// <inheritdoc/>
        public State State(string name)
        {
            modifier.State(name, out State state);
            return state;
        }

        /// <inheritdoc/>
        public void DuringAny<T>(Event<T> @event, StateMachineCondition<TSaga, T> filter, Func<EventActivityBinder<TSaga, T>, EventActivityBinder<TSaga, T>> configure)
            where T : class
        {
            modifier.DuringAny().When(@event, filter, configure).CommitActivities();
        }

    }

}
