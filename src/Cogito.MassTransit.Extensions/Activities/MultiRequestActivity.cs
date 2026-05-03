using System;
using System.Threading.Tasks;

using MassTransit;

namespace Cogito.MassTransit.Extensions.Activities
{

    /// <summary>
    /// Sends an enumerable of request messages and registers tracking state for each.
    /// </summary>
    /// <typeparam name="TSaga"></typeparam>
    /// <typeparam name="TMessage"></typeparam>
    /// <typeparam name="TState"></typeparam>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    class MultiRequestActivity<TSaga, TMessage, TState, TRequest, TResponse> :
        MultiRequestActivityImpl<TSaga, TState, TRequest, TResponse>,
        IStateMachineActivity<TSaga, TMessage>
        where TSaga : class, SagaStateMachineInstance
        where TMessage : class
        where TRequest : class
        where TResponse : class
    {

        readonly EventMultiMessageFactory<TSaga, TMessage, TRequest> messageFactory;
        readonly AsyncEventMultiMessageFactory<TSaga, TMessage, TRequest> asyncMessageFactory;
        readonly ServiceAddressProvider<TSaga, TMessage> serviceAddressProvider;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="messageFactory"></param>
        public MultiRequestActivity(MultiRequest<TSaga, TState, TRequest, TResponse> request, EventMultiMessageFactory<TSaga, TMessage, TRequest> messageFactory) :
            base(request)
        {
            this.messageFactory = messageFactory ?? throw new ArgumentNullException(nameof(messageFactory));
            this.serviceAddressProvider = context => request.Settings.ServiceAddress;
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="serviceAddressProvider"></param>
        /// <param name="messageFactory"></param>
        public MultiRequestActivity(MultiRequest<TSaga, TState, TRequest, TResponse> request, ServiceAddressProvider<TSaga, TMessage> serviceAddressProvider, EventMultiMessageFactory<TSaga, TMessage, TRequest> messageFactory) :
            base(request)
        {
            if (serviceAddressProvider == null) throw new ArgumentNullException(nameof(serviceAddressProvider));
            this.messageFactory = messageFactory ?? throw new ArgumentNullException(nameof(messageFactory));
            this.serviceAddressProvider = context => serviceAddressProvider(context) ?? request.Settings.ServiceAddress;
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="asyncMessageFactory"></param>
        public MultiRequestActivity(MultiRequest<TSaga, TState, TRequest, TResponse> request, AsyncEventMultiMessageFactory<TSaga, TMessage, TRequest> asyncMessageFactory) :
            base(request)
        {
            this.asyncMessageFactory = asyncMessageFactory ?? throw new ArgumentNullException(nameof(asyncMessageFactory));
            this.serviceAddressProvider = context => request.Settings.ServiceAddress;
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="serviceAddressProvider"></param>
        /// <param name="asyncMessageFactory"></param>
        public MultiRequestActivity(MultiRequest<TSaga, TState, TRequest, TResponse> request, ServiceAddressProvider<TSaga, TMessage> serviceAddressProvider, AsyncEventMultiMessageFactory<TSaga, TMessage, TRequest> asyncMessageFactory) :
            base(request)
        {
            if (serviceAddressProvider == null) throw new ArgumentNullException(nameof(serviceAddressProvider));
            this.asyncMessageFactory = asyncMessageFactory ?? throw new ArgumentNullException(nameof(asyncMessageFactory));
            this.serviceAddressProvider = context => serviceAddressProvider(context) ?? request.Settings.ServiceAddress;
        }

        void IVisitable.Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public async Task Execute(BehaviorContext<TSaga, TMessage> context, IBehavior<TSaga, TMessage> next)
        {
            if (messageFactory != null)
                foreach (var message in messageFactory(context))
                    await SendRequest(context, message, serviceAddressProvider(context)).ConfigureAwait(false);

            if (asyncMessageFactory != null)
                await foreach (var message in asyncMessageFactory(context).ConfigureAwait(false))
                    await SendRequest(context, message, serviceAddressProvider(context)).ConfigureAwait(false);

            await next.Execute(context).ConfigureAwait(false);
        }

        public Task Faulted<TException>(BehaviorExceptionContext<TSaga, TMessage, TException> context, IBehavior<TSaga, TMessage> next)
            where TException : Exception
        {
            return next.Faulted(context);
        }

    }

}
