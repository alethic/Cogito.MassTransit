using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Cogito.MassTransit.Events;
using Cogito.MassTransit.Extensions.Activities;

using MassTransit;

namespace Cogito.MassTransit.Extensions.Internal
{

    /// <summary>
    /// Declares a multi-request on a state machine: its events, its pending state, and the activities that track
    /// each item through to the finished signal. Shared by every way of building a state machine.
    /// </summary>
    static class MultiRequestDeclaration
    {

        /// <summary>
        /// Declares a multi-request named <paramref name="name"/> through the specified declarer. Its events and
        /// state are named <c>{name}.{member}</c>, matching the names MassTransit gives to nested properties.
        /// </summary>
        /// <typeparam name="TSaga"></typeparam>
        /// <typeparam name="TState"></typeparam>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="declarer"></param>
        /// <param name="name"></param>
        /// <param name="itemsExpression"></param>
        /// <param name="requestIdExpression"></param>
        /// <param name="accessor"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static MultiRequest<TSaga, TState, TRequest, TResponse> Declare<TSaga, TState, TRequest, TResponse>(
            IMultiRequestDeclarer<TSaga> declarer,
            string name,
            Expression<Func<TSaga, IEnumerable<TState>>> itemsExpression,
            Expression<Func<TState, Guid?>> requestIdExpression,
            IMultiRequestStateAccessor<TSaga, TState, TRequest, TResponse> accessor,
            MultiRequestSettings settings)
            where TSaga : class, SagaStateMachineInstance
            where TRequest : class
            where TResponse : class
        {
            if (declarer == null)
                throw new ArgumentNullException(nameof(declarer));
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("A multi-request must have a name.", nameof(name));
            if (itemsExpression == null)
                throw new ArgumentNullException(nameof(itemsExpression));
            if (requestIdExpression == null)
                throw new ArgumentNullException(nameof(requestIdExpression));
            if (accessor == null)
                throw new ArgumentNullException(nameof(accessor));
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

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

            var request = new StateMachineMultiRequest<TSaga, TState, TRequest, TResponse>(name, filterExpression, itemsExpression, requestIdExpression, accessor, settings);

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

            // the finished event is raised from within the state machine, never received, but a named event must declare
            // a correlation: correlate on the header, and keep the endpoint from subscribing to the message
            request.Finished = declarer.Event<MultiRequestFinished<TRequest, TResponse>>($"{name}.{nameof(request.Finished)}", x =>
            {
                x.CorrelateById(context => context.CorrelationId ?? Guid.Empty);
                x.ConfigureConsumeTopology = false;
            });
            request.Completed = declarer.Event<TResponse>($"{name}.{nameof(request.Completed)}", x => x.CorrelateBy(completedExpression));
            request.Faulted = declarer.Event<Fault<TRequest>>($"{name}.{nameof(request.Faulted)}", x => x.CorrelateBy(faultedExpression));
            request.TimeoutExpired = declarer.Event<RequestTimeoutExpired<TRequest>>($"{name}.{nameof(request.TimeoutExpired)}", x => x.CorrelateBy(timeoutExpiredExpression));
            request.FinishedSignal = declarer.Event<MultiRequestFinishedSignal>($"{name}.{nameof(request.FinishedSignal)}", x => x.CorrelateById(m => (Guid)m.CorrelationId));
            request.Pending = declarer.State($"{name}.{nameof(request.Pending)}");

            declarer.DuringAny(request.Completed, request.CompletedEventFilter, b => b
                .Add(new MultiRequestItemCompletedActivity<TSaga, TState, TRequest, TResponse>(request))
                .Add(new MultiRequestItemFinishedActivity<TSaga, TState, TRequest, TResponse>(request))
                .Add(new MultiRequestCancelItemTimeoutActivity<TSaga, TState, TRequest, TResponse>(request)));

            declarer.DuringAny(request.Faulted, request.FaultedEventFilter, b => b
                .Add(new MultiRequestItemFaultedActivity<TSaga, TState, TRequest, TResponse>(request))
                .Add(new MultiRequestItemFinishedActivity<TSaga, TState, TRequest, TResponse>(request))
                .Add(new MultiRequestCancelItemTimeoutActivity<TSaga, TState, TRequest, TResponse>(request)));

            declarer.DuringAny(request.TimeoutExpired, request.RequestTimeoutExpiredEventFilter, b => b
                .Add(new MultiRequestItemTimeoutExpiredActivity<TSaga, TState, TRequest, TResponse>(request))
                .Add(new MultiRequestItemFinishedActivity<TSaga, TState, TRequest, TResponse>(request))
                .Add(new MultiRequestCancelItemTimeoutActivity<TSaga, TState, TRequest, TResponse>(request)));

            declarer.DuringAny(request.FinishedSignal, request.FinishedSignalEventFilter, b => b
                .Add(new MultiRequestFinishedActivity<TSaga, TState, TRequest, TResponse>(request)));

            return request;
        }

    }

}
