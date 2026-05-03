using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Cogito.MassTransit.Extensions.Activities;
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

            // parameters reused within expressions
            var instanceParameter = Expression.Parameter(typeof(TSaga), "instance");
            var itemParameter = Expression.Parameter(typeof(TState), "item");
            var requestIdParameter = Expression.Parameter(typeof(Guid), "requestId");

            // filters an instance to that which contains the request
            var filterExpression = Expression.Lambda<Func<TSaga, Guid, bool>>(
                Expression.Call(
                    typeof(Enumerable),
                    nameof(Enumerable.Any),
                    new[] { typeof(TState) },
                    itemsExpression.Body.Replace(itemsExpression.Parameters[0], instanceParameter),
                    Expression.Lambda<Func<TState, bool>>(
                        Expression.Equal(
                            Expression.Convert(requestIdExpression.Body.Replace(requestIdExpression.Parameters[0], itemParameter), typeof(Guid?)),
                            Expression.Convert(requestIdParameter, typeof(Guid?))),
                        itemParameter)),
                instanceParameter,
                requestIdParameter);

            var request = new StateMachineMultiRequest<TSaga, TState, TRequest, TResponse>(property.Name, filterExpression, itemsExpression, requestIdExpression, accessor, settings);

            InitializeMultiRequest(this, property, request);

            Event(propertyExpression, x => x.Finished);

            // filters instances for the Completed event
            var completedContextParameter = Expression.Parameter(typeof(ConsumeContext<TResponse>), "context");
            var completedExpression = Expression.Lambda<Func<TSaga, ConsumeContext<TResponse>, bool>>(
                Expression.Call(
                    typeof(Enumerable),
                    nameof(Enumerable.Any),
                    new[] { typeof(TState) },
                    itemsExpression.Body.Replace(itemsExpression.Parameters[0], instanceParameter),
                    Expression.Lambda<Func<TState, bool>>(
                        Expression.Equal(
                            requestIdExpression.Body.Replace(requestIdExpression.Parameters[0], itemParameter),
                            Expression.Property(completedContextParameter, typeof(MessageContext), nameof(MessageContext.RequestId))),
                        itemParameter)),
                instanceParameter,
                completedContextParameter);

            // filters instances for the Faulted event
            var faultedContextParameter = Expression.Parameter(typeof(ConsumeContext<Fault<TRequest>>), "context");
            var faultedExpression = Expression.Lambda<Func<TSaga, ConsumeContext<Fault<TRequest>>, bool>>(
                Expression.Call(
                    typeof(Enumerable),
                    nameof(Enumerable.Any),
                    new[] { typeof(TState) },
                    itemsExpression.Body.Replace(itemsExpression.Parameters[0], instanceParameter),
                    Expression.Lambda<Func<TState, bool>>(
                        Expression.Equal(
                            requestIdExpression.Body.Replace(requestIdExpression.Parameters[0], itemParameter),
                            Expression.Property(faultedContextParameter, typeof(MessageContext), nameof(MessageContext.RequestId))),
                        itemParameter)),
                instanceParameter,
                faultedContextParameter);

            // filters instances for the TimeoutExpired event
            var timeoutExpiredContextParameter = Expression.Parameter(typeof(ConsumeContext<RequestTimeoutExpired<TRequest>>), "context");
            var timeoutExpiredExpression = Expression.Lambda<Func<TSaga, ConsumeContext<RequestTimeoutExpired<TRequest>>, bool>>(
                Expression.Call(
                    typeof(Enumerable),
                    nameof(Enumerable.Any),
                    new[] { typeof(TState) },
                    itemsExpression.Body.Replace(itemsExpression.Parameters[0], instanceParameter),
                    Expression.Lambda<Func<TState, bool>>(
                        Expression.Equal(
                            requestIdExpression.Body.Replace(requestIdExpression.Parameters[0], itemParameter),
                            Expression.Convert(
                                Expression.Property(
                                    Expression.Property(timeoutExpiredContextParameter, typeof(ConsumeContext<RequestTimeoutExpired<TRequest>>), nameof(ConsumeContext<RequestTimeoutExpired<TRequest>>.Message)),
                                    typeof(RequestTimeoutExpired<TRequest>),
                                    nameof(RequestTimeoutExpired<TRequest>.RequestId)),
                                typeof(Guid?))),
                        itemParameter)),
                instanceParameter,
                timeoutExpiredContextParameter);

            Event(propertyExpression, x => x.Completed, x => x.CorrelateBy(completedExpression));
            Event(propertyExpression, x => x.Faulted, x => x.CorrelateBy(faultedExpression));
            Event(propertyExpression, x => x.TimeoutExpired, x => x.CorrelateBy(timeoutExpiredExpression));
            Event(propertyExpression, x => x.FinishedSignal, x => x.CorrelateById(m => (Guid)m.CorrelationId));

            State(propertyExpression, x => x.Pending);

            DuringAny(
                When(request.Completed, request.CompletedEventFilter)
                    .Add(new MultiRequestItemCompletedActivity<TSaga, TState, TRequest, TResponse>(request))
                    .Add(new MultiRequestItemFinishedActivity<TSaga, TState, TRequest, TResponse>(request))
                    .Add(new MultiRequestCancelItemTimeoutActivity<TSaga, TState, TRequest, TResponse>(request)),
                When(request.Faulted, request.FaultedEventFilter)
                    .Add(new MultiRequestItemFaultedActivity<TSaga, TState, TRequest, TResponse>(request))
                    .Add(new MultiRequestItemFinishedActivity<TSaga, TState, TRequest, TResponse>(request))
                    .Add(new MultiRequestCancelItemTimeoutActivity<TSaga, TState, TRequest, TResponse>(request)),
                When(request.TimeoutExpired, request.RequestTimeoutExpiredEventFilter)
                    .Add(new MultiRequestItemTimeoutExpiredActivity<TSaga, TState, TRequest, TResponse>(request))
                    .Add(new MultiRequestItemFinishedActivity<TSaga, TState, TRequest, TResponse>(request))
                    .Add(new MultiRequestCancelItemTimeoutActivity<TSaga, TState, TRequest, TResponse>(request)),
                When(request.FinishedSignal, request.FinishedSignalEventFilter)
                    .Add(new MultiRequestFinishedActivity<TSaga, TState, TRequest, TResponse>(request)));
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

    }

}
