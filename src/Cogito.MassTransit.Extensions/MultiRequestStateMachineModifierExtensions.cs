using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using Cogito.MassTransit.Extensions.Internal;

using MassTransit;

namespace Cogito.MassTransit
{

    /// <summary>
    /// Declares multi-requests on a state machine built with <see cref="MassTransitStateMachine{TInstance}.New"/>.
    /// </summary>
    public static class MultiRequestStateMachineModifierExtensions
    {

        /// <summary>
        /// Declares a multi-request on the state machine, using the default settings.
        /// </summary>
        /// <typeparam name="TSaga"></typeparam>
        /// <typeparam name="TState"></typeparam>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="modifier"></param>
        /// <param name="name">The name of the multi-request, which prefixes the names of its events and state.</param>
        /// <param name="itemsExpression"></param>
        /// <param name="requestIdExpression"></param>
        /// <param name="accessor"></param>
        /// <param name="request">The declared multi-request.</param>
        /// <returns></returns>
        public static IStateMachineModifier<TSaga> MultiRequest<TSaga, TState, TRequest, TResponse>(
            this IStateMachineModifier<TSaga> modifier,
            string name,
            Expression<Func<TSaga, IEnumerable<TState>>> itemsExpression,
            Expression<Func<TState, Guid?>> requestIdExpression,
            IMultiRequestStateAccessor<TSaga, TState, TRequest, TResponse> accessor,
            out MultiRequest<TSaga, TState, TRequest, TResponse> request)
            where TSaga : class, SagaStateMachineInstance
            where TRequest : class
            where TResponse : class
        {
            return modifier.MultiRequest(name, itemsExpression, requestIdExpression, accessor, new StateMachineMultiRequestConfigurator<TRequest>(), out request);
        }

        /// <summary>
        /// Declares a multi-request on the state machine, allowing inline configuration.
        /// </summary>
        /// <typeparam name="TSaga"></typeparam>
        /// <typeparam name="TState"></typeparam>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="modifier"></param>
        /// <param name="name">The name of the multi-request, which prefixes the names of its events and state.</param>
        /// <param name="itemsExpression"></param>
        /// <param name="requestIdExpression"></param>
        /// <param name="accessor"></param>
        /// <param name="configureRequest"></param>
        /// <param name="request">The declared multi-request.</param>
        /// <returns></returns>
        public static IStateMachineModifier<TSaga> MultiRequest<TSaga, TState, TRequest, TResponse>(
            this IStateMachineModifier<TSaga> modifier,
            string name,
            Expression<Func<TSaga, IEnumerable<TState>>> itemsExpression,
            Expression<Func<TState, Guid?>> requestIdExpression,
            IMultiRequestStateAccessor<TSaga, TState, TRequest, TResponse> accessor,
            Action<IMultiRequestConfigurator> configureRequest,
            out MultiRequest<TSaga, TState, TRequest, TResponse> request)
            where TSaga : class, SagaStateMachineInstance
            where TRequest : class
            where TResponse : class
        {
            var configurator = new StateMachineMultiRequestConfigurator<TRequest>();
            configureRequest?.Invoke(configurator);
            return modifier.MultiRequest(name, itemsExpression, requestIdExpression, accessor, configurator, out request);
        }

        /// <summary>
        /// Declares a multi-request on the state machine, with the specified settings.
        /// </summary>
        /// <typeparam name="TSaga"></typeparam>
        /// <typeparam name="TState"></typeparam>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="modifier"></param>
        /// <param name="name">The name of the multi-request, which prefixes the names of its events and state.</param>
        /// <param name="itemsExpression"></param>
        /// <param name="requestIdExpression"></param>
        /// <param name="accessor"></param>
        /// <param name="settings"></param>
        /// <param name="request">The declared multi-request.</param>
        /// <returns></returns>
        public static IStateMachineModifier<TSaga> MultiRequest<TSaga, TState, TRequest, TResponse>(
            this IStateMachineModifier<TSaga> modifier,
            string name,
            Expression<Func<TSaga, IEnumerable<TState>>> itemsExpression,
            Expression<Func<TState, Guid?>> requestIdExpression,
            IMultiRequestStateAccessor<TSaga, TState, TRequest, TResponse> accessor,
            MultiRequestSettings settings,
            out MultiRequest<TSaga, TState, TRequest, TResponse> request)
            where TSaga : class, SagaStateMachineInstance
            where TRequest : class
            where TResponse : class
        {
            if (modifier == null)
                throw new ArgumentNullException(nameof(modifier));

            request = MultiRequestDeclaration.Declare(new StateMachineModifierMultiRequestDeclarer<TSaga>(modifier), name, itemsExpression, requestIdExpression, accessor, settings);
            return modifier;
        }

    }

}
