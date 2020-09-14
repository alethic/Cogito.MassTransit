using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Automatonymous;
using Automatonymous.Events;

using Cogito.MassTransit.Automatonymous.Activities;
using Cogito.MassTransit.Automatonymous.MultiRequests;
using Cogito.MassTransit.Automatonymous.SagaConfigurators;

using MassTransit;
using MassTransit.Internals.Extensions;

namespace Cogito.MassTransit.Automatonymous
{

    /// <summary>
    /// A MassTransit state machine adds functionality on top of Automatonymous supporting things like request/response,
    /// and correlating events to the state machine, as well as retry and policy configuration.
    /// </summary>
    /// <typeparam name="TInstance"></typeparam>
    public abstract class MassTransitStateMachine<TInstance> :
        global::Automatonymous.MassTransitStateMachine<TInstance>
        where TInstance : class, SagaStateMachineInstance
    {

        /// <summary>
        /// Sets the <see cref="IMultiRequest{TInstance, TKey, TRequest, TResponse}"/> property.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="stateMachine"></param>
        /// <param name="property"></param>
        /// <param name="request"></param>
        static void InitializeMultiRequest<TKey, TRequest, TResponse>(
            AutomatonymousStateMachine<TInstance> stateMachine,
            PropertyInfo property,
            IMultiRequest<TInstance, TKey, TRequest, TResponse> request)
            where TRequest : class
            where TResponse : class
        {
            if (property.CanWrite)
                property.SetValue(stateMachine, request);
            else if (ConfigurationHelpers.TryGetBackingField(stateMachine.GetType().GetTypeInfo(), property, out var backingField))
                backingField.SetValue(stateMachine, request);
            else
                throw new ArgumentException($"The multi-request property is not writable: {property.Name}");
        }

        /// <summary>
        /// Declares a request that is sent by the state machine to a service, and the associated response, fault, and
        /// timeout handling. The property is initialized with the fully built Request. The request must be declared
        /// before it is used in the state/event declaration statements.
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="propertyExpression"></param>
        /// <param name="itemsExpression"></param>
        /// <param name="requestIdExpression"></param>
        /// <param name="adaptor"></param>
        /// <param name="configureRequest"></param>
        protected void MultiRequest<TState, TRequest, TResponse>(
            Expression<Func<IMultiRequest<TInstance, TState, TRequest, TResponse>>> propertyExpression,
            Expression<Func<TInstance, IEnumerable<TState>>> itemsExpression,
            Expression<Func<TState, Guid?>> requestIdExpression,
            IMultiRequestStateAccessor<TInstance, TState, TRequest, TResponse> adaptor)
            where TRequest : class
            where TResponse : class
        {
            MultiRequest(propertyExpression, itemsExpression, requestIdExpression, adaptor, new StateMachineMultiRequestConfigurator<TRequest>());
        }

        /// <summary>
        /// Declares a request that is sent by the state machine to a service, and the associated response, fault, and
        /// timeout handling. The property is initialized with the fully built Request. The request must be declared
        /// before it is used in the state/event declaration statements.
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="propertyExpression"></param>
        /// <param name="itemsExpression"></param>
        /// <param name="requestIdExpression"></param>
        /// <param name="adaptor"></param>
        /// <param name="configureRequest"></param>
        protected void MultiRequest<TState, TRequest, TResponse>(
            Expression<Func<IMultiRequest<TInstance, TState, TRequest, TResponse>>> propertyExpression,
            Expression<Func<TInstance, IEnumerable<TState>>> itemsExpression,
            Expression<Func<TState, Guid?>> requestIdExpression,
            IMultiRequestStateAccessor<TInstance, TState, TRequest, TResponse> adaptor,
            Action<IMultiRequestConfigurator> configureRequest = null)
            where TRequest : class
            where TResponse : class
        {
            var configurator = new StateMachineMultiRequestConfigurator<TRequest>();
            configureRequest?.Invoke(configurator);
            MultiRequest(propertyExpression, itemsExpression, requestIdExpression, adaptor, configurator);
        }

        /// <summary>
        /// Declares a request that is sent by the state machine to a service, and the associated response, fault, and
        /// timeout handling. The property is initialized with the fully built Request. The request must be declared
        /// before it is used in the state/event declaration statements.
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="propertyExpression"></param>
        /// <param name="itemsExpression"></param>
        /// <param name="requestIdExpression"></param>
        /// <param name="adaptor"></param>
        /// <param name="settings"></param>
        protected void MultiRequest<TState, TRequest, TResponse>(
            Expression<Func<IMultiRequest<TInstance, TState, TRequest, TResponse>>> propertyExpression,
            Expression<Func<TInstance, IEnumerable<TState>>> itemsExpression,
            Expression<Func<TState, Guid?>> requestIdExpression,
            IMultiRequestStateAccessor<TInstance, TState, TRequest, TResponse> adaptor,
            MultiRequestSettings settings = null)
            where TRequest : class
            where TResponse : class
        {
            var property = propertyExpression.GetPropertyInfo();

            // parameters reused within expressions
            var instanceParameter = Expression.Parameter(typeof(TInstance), "instance");
            var itemParameter = Expression.Parameter(typeof(TState), "item");
            var requestIdParameter = Expression.Parameter(typeof(Guid), "requestId");

            // filters an instance to that which contains the request
            var filterExpression = Expression.Lambda<Func<TInstance, Guid, bool>>(
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

            var request = new StateMachineMultiRequest<TInstance, TState, TRequest, TResponse>(property.Name, filterExpression, itemsExpression, requestIdExpression, adaptor, settings);

            InitializeMultiRequest(this, property, request);

            Event(propertyExpression, x => x.Finished);

            // filters instances for the Completed event
            var completedContextParameter = Expression.Parameter(typeof(ConsumeContext<TResponse>), "context");
            var completedExpression = Expression.Lambda<Func<TInstance, ConsumeContext<TResponse>, bool>>(
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
            var faultedExpression = Expression.Lambda<Func<TInstance, ConsumeContext<Fault<TRequest>>, bool>>(
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
            var timeoutExpiredExpression = Expression.Lambda<Func<TInstance, ConsumeContext<RequestTimeoutExpired<TRequest>>, bool>>(
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

            State(propertyExpression, x => x.Pending);

            DuringAny(
                When(request.Completed, request.CompletedEventFilter)
                    .Add(new MultiRequestItemCompletedActivity<TInstance, TState, TRequest, TResponse>(request))
                    .Add(new MultiRequestItemFinishedActivity<TInstance, TState, TRequest, TResponse>(request))
                    .Add(new MultiRequestCancelItemTimeoutActivity<TInstance, TState, TRequest, TResponse>(request)),
                When(request.Faulted, request.FaultedEventFilter)
                    .Add(new MultiRequestItemFaultedActivity<TInstance, TState, TRequest, TResponse>(request))
                    .Add(new MultiRequestItemFinishedActivity<TInstance, TState, TRequest, TResponse>(request))
                    .Add(new MultiRequestCancelItemTimeoutActivity<TInstance, TState, TRequest, TResponse>(request)),
                When(request.TimeoutExpired, request.RequestTimeoutExpiredEventFilter)
                    .Add(new MultiRequestItemTimeoutExpiredActivity<TInstance, TState, TRequest, TResponse>(request))
                    .Add(new MultiRequestItemFinishedActivity<TInstance, TState, TRequest, TResponse>(request))
                    .Add(new MultiRequestCancelItemTimeoutActivity<TInstance, TState, TRequest, TResponse>(request)));
        }

    }

}
