using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

using Cogito.MassTransit.Extensions.Internal;

using MassTransit;

namespace Cogito.MassTransit
{

    /// <summary>
    /// A MassTransit state machine that adds support for declaring multi-requests on top of the
    /// stock MassTransit state machine.
    /// </summary>
    /// <typeparam name="TSaga"></typeparam>
    public abstract class MassTransitStateMachine<TSaga> :
        global::MassTransit.MassTransitStateMachine<TSaga>
        where TSaga : class, SagaStateMachineInstance
    {

        /// <summary>
        /// Sets the <see cref="MultiRequest{TSaga, TKey, TRequest, TResponse}"/> property on the state machine.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="stateMachine"></param>
        /// <param name="property"></param>
        /// <param name="request"></param>
        static void InitializeMultiRequest<TKey, TRequest, TResponse>(
            global::MassTransit.MassTransitStateMachine<TSaga> stateMachine,
            PropertyInfo property,
            MultiRequest<TSaga, TKey, TRequest, TResponse> request)
            where TRequest : class
            where TResponse : class
        {
            if (property.CanWrite)
                property.SetValue(stateMachine, request);
            else if (TryGetBackingField(property, out var backingField))
                backingField.SetValue(stateMachine, request);
            else
                throw new ArgumentException($"The multi-request property is not writable: {property.Name}");
        }

        /// <summary>
        /// Attempts to locate the compiler-generated backing field for the given auto-property.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="backingField"></param>
        /// <returns></returns>
        static bool TryGetBackingField(PropertyInfo property, out FieldInfo backingField)
        {
            backingField = property.DeclaringType?.GetField($"<{property.Name}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            return backingField != null;
        }

        /// <summary>
        /// Declares a multi-request on the state machine, using the default settings.
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="propertyExpression"></param>
        /// <param name="itemsExpression"></param>
        /// <param name="requestIdExpression"></param>
        /// <param name="accessor"></param>
        protected void MultiRequest<TState, TRequest, TResponse>(
            Expression<Func<MultiRequest<TSaga, TState, TRequest, TResponse>>> propertyExpression,
            Expression<Func<TSaga, IEnumerable<TState>>> itemsExpression,
            Expression<Func<TState, Guid?>> requestIdExpression,
            IMultiRequestStateAccessor<TSaga, TState, TRequest, TResponse> accessor)
            where TRequest : class
            where TResponse : class
        {
            MultiRequest(propertyExpression, itemsExpression, requestIdExpression, accessor, new StateMachineMultiRequestConfigurator<TRequest>());
        }

        /// <summary>
        /// Declares a multi-request on the state machine, allowing inline configuration.
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="propertyExpression"></param>
        /// <param name="itemsExpression"></param>
        /// <param name="requestIdExpression"></param>
        /// <param name="accessor"></param>
        /// <param name="configureRequest"></param>
        protected void MultiRequest<TState, TRequest, TResponse>(
            Expression<Func<MultiRequest<TSaga, TState, TRequest, TResponse>>> propertyExpression,
            Expression<Func<TSaga, IEnumerable<TState>>> itemsExpression,
            Expression<Func<TState, Guid?>> requestIdExpression,
            IMultiRequestStateAccessor<TSaga, TState, TRequest, TResponse> accessor,
            Action<IMultiRequestConfigurator>? configureRequest = null)
            where TRequest : class
            where TResponse : class
        {
            var configurator = new StateMachineMultiRequestConfigurator<TRequest>();
            configureRequest?.Invoke(configurator);
            MultiRequest(propertyExpression, itemsExpression, requestIdExpression, accessor, configurator);
        }

        /// <summary>
        /// Declares a multi-request on the state machine, with the specified settings.
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="propertyExpression"></param>
        /// <param name="itemsExpression"></param>
        /// <param name="requestIdExpression"></param>
        /// <param name="accessor"></param>
        /// <param name="settings"></param>
        protected void MultiRequest<TState, TRequest, TResponse>(
            Expression<Func<MultiRequest<TSaga, TState, TRequest, TResponse>>> propertyExpression,
            Expression<Func<TSaga, IEnumerable<TState>>> itemsExpression,
            Expression<Func<TState, Guid?>> requestIdExpression,
            IMultiRequestStateAccessor<TSaga, TState, TRequest, TResponse> accessor,
            MultiRequestSettings? settings = null)
            where TRequest : class
            where TResponse : class
        {
            // default settings
            if (settings == null)
                settings = new StateMachineMultiRequestConfigurator<TRequest>();

            var property = GetPropertyInfo(propertyExpression);
            var request = MultiRequestDeclaration.Declare(new Declarer(this), property.Name, itemsExpression, requestIdExpression, accessor, settings);
            InitializeMultiRequest(this, property, request);
        }

        /// <summary>
        /// Resolves the <see cref="PropertyInfo"/> referenced by a property-access lambda.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        static PropertyInfo GetPropertyInfo<T>(Expression<Func<T>> expression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            var body = expression.Body as MemberExpression;
            if (body?.Member is PropertyInfo property)
                return property;

            throw new ArgumentException("Expression must reference a property.", nameof(expression));
        }

        /// <summary>
        /// Declares the parts of a multi-request through the protected members of the state machine.
        /// </summary>
        class Declarer : IMultiRequestDeclarer<TSaga>
        {

            readonly MassTransitStateMachine<TSaga> machine;

            /// <summary>
            /// Initializes a new instance.
            /// </summary>
            /// <param name="machine"></param>
            public Declarer(MassTransitStateMachine<TSaga> machine)
            {
                this.machine = machine ?? throw new ArgumentNullException(nameof(machine));
            }

            /// <inheritdoc/>
            public Event<T> Event<T>(string name, Action<IEventCorrelationConfigurator<TSaga, T>>? configure = null)
                where T : class
            {
                return configure != null ? machine.Event(name, configure) : machine.Event<T>(name);
            }

            /// <inheritdoc/>
            public State State(string name)
            {
                return machine.State(name);
            }

            /// <inheritdoc/>
            public void DuringAny<T>(Event<T> @event, StateMachineCondition<TSaga, T> filter, Func<EventActivityBinder<TSaga, T>, EventActivityBinder<TSaga, T>> configure)
                where T : class
            {
                machine.DuringAny(configure(machine.When(@event, filter)));
            }

        }

    }

}
