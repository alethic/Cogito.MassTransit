using System;
using System.Threading.Tasks;

using Automatonymous;

namespace Cogito.MassTransit.Automatonymous.Activities
{

    public class MultiRequestActivity<TInstance, TState, TRequest, TResponse> :
        MultiRequestActivityImpl<TInstance, TState, TRequest, TResponse>,
        Activity<TInstance>
        where TInstance : class, SagaStateMachineInstance
        where TRequest : class
        where TResponse : class
    {

        readonly EventMultiMessageFactory<TInstance, TRequest> messageFactory;
        readonly AsyncEventMultiMessageFactory<TInstance, TRequest> asyncMessageFactory;
        readonly ServiceAddressProvider<TInstance> serviceAddressProvider;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="messageFactory"></param>
        public MultiRequestActivity(IMultiRequest<TInstance, TState, TRequest, TResponse> request, EventMultiMessageFactory<TInstance, TRequest> messageFactory) :
            base(request)
        {
            this.messageFactory = messageFactory;
            serviceAddressProvider = context => request.Settings.ServiceAddress;
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="serviceAddressProvider"></param>
        /// <param name="messageFactory"></param>
        public MultiRequestActivity(IMultiRequest<TInstance, TState, TRequest, TResponse> request, ServiceAddressProvider<TInstance> serviceAddressProvider, EventMultiMessageFactory<TInstance, TRequest> messageFactory) :
            base(request)
        {
            this.messageFactory = messageFactory;
            serviceAddressProvider = context => serviceAddressProvider(context) ?? request.Settings.ServiceAddress;
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="asyncMessageFactory"></param>
        public MultiRequestActivity(IMultiRequest<TInstance, TState, TRequest, TResponse> request, AsyncEventMultiMessageFactory<TInstance, TRequest> asyncMessageFactory) :
            base(request)
        {
            this.asyncMessageFactory = asyncMessageFactory;
            serviceAddressProvider = context => request.Settings.ServiceAddress;
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="serviceAddressProvider"></param>
        /// <param name="asyncMessageFactory"></param>
        public MultiRequestActivity(IMultiRequest<TInstance, TState, TRequest, TResponse> request, ServiceAddressProvider<TInstance> serviceAddressProvider, AsyncEventMultiMessageFactory<TInstance, TRequest> asyncMessageFactory) :
            base(request)
        {
            this.asyncMessageFactory = asyncMessageFactory;
            serviceAddressProvider = context => serviceAddressProvider(context) ?? request.Settings.ServiceAddress;
        }

        public void Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        async Task Execute(BehaviorContext<TInstance> context)
        {
            var consumeContext = context.CreateConsumeContext();

            if (messageFactory != null)
                foreach (var message in messageFactory(consumeContext))
                    await SendRequest(context, consumeContext, message, serviceAddressProvider(consumeContext)).ConfigureAwait(false);

            if (asyncMessageFactory != null)
                await foreach (var message in asyncMessageFactory(consumeContext))
                    await SendRequest(context, consumeContext, message, serviceAddressProvider(consumeContext)).ConfigureAwait(false);
        }

        async Task Activity<TInstance>.Execute(BehaviorContext<TInstance> context, Behavior<TInstance> next)
        {
            await Execute(context);
            await next.Execute(context).ConfigureAwait(false);
        }

        async Task Activity<TInstance>.Execute<T>(BehaviorContext<TInstance, T> context, Behavior<TInstance, T> next)
        {
            await Execute(context);
            await next.Execute(context).ConfigureAwait(false);
        }

        public Task Faulted<TException>(BehaviorExceptionContext<TInstance, TException> context, Behavior<TInstance> next)
            where TException : Exception
        {
            return next.Faulted(context);
        }

        public Task Faulted<T, TException>(BehaviorExceptionContext<TInstance, T, TException> context, Behavior<TInstance, T> next)
            where TException : Exception
        {
            return next.Faulted(context);
        }

    }

    public class MultiRequestActivity<TInstance, TData, TState, TRequest, TResponse> :
        MultiRequestActivityImpl<TInstance, TState, TRequest, TResponse>,
        Activity<TInstance, TData>
        where TInstance : class, SagaStateMachineInstance
        where TData : class
        where TRequest : class
        where TResponse : class
    {

        readonly EventMultiMessageFactory<TInstance, TData, TRequest> messageFactory;
        readonly AsyncEventMultiMessageFactory<TInstance, TData, TRequest> asyncMessageFactory;
        readonly ServiceAddressProvider<TInstance, TData> serviceAddressProvider;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="messageFactory"></param>
        public MultiRequestActivity(IMultiRequest<TInstance, TState, TRequest, TResponse> request, EventMultiMessageFactory<TInstance, TData, TRequest> messageFactory) :
            base(request)
        {
            this.messageFactory = messageFactory;
            serviceAddressProvider = context => request.Settings.ServiceAddress;
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="serviceAddressProvider"></param>
        /// <param name="messageFactory"></param>
        public MultiRequestActivity(IMultiRequest<TInstance, TState, TRequest, TResponse> request, ServiceAddressProvider<TInstance, TData> serviceAddressProvider, EventMultiMessageFactory<TInstance, TData, TRequest> messageFactory) :
            base(request)
        {
            this.messageFactory = messageFactory;
            serviceAddressProvider = context => serviceAddressProvider(context) ?? request.Settings.ServiceAddress;
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="asyncMessageFactory"></param>
        public MultiRequestActivity(IMultiRequest<TInstance, TState, TRequest, TResponse> request, AsyncEventMultiMessageFactory<TInstance, TData, TRequest> asyncMessageFactory) :
            base(request)
        {
            this.asyncMessageFactory = asyncMessageFactory;
            serviceAddressProvider = context => request.Settings.ServiceAddress;
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="serviceAddressProvider"></param>
        /// <param name="asyncMessageFactory"></param>
        public MultiRequestActivity(IMultiRequest<TInstance, TState, TRequest, TResponse> request, ServiceAddressProvider<TInstance, TData> serviceAddressProvider, AsyncEventMultiMessageFactory<TInstance, TData, TRequest> asyncMessageFactory) :
            base(request)
        {
            this.asyncMessageFactory = asyncMessageFactory;
            serviceAddressProvider = context => serviceAddressProvider(context) ?? request.Settings.ServiceAddress;
        }

        public void Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public async Task Execute(BehaviorContext<TInstance, TData> context, Behavior<TInstance, TData> next)
        {
            var consumeContext = context.CreateConsumeContext();

            if (messageFactory != null)
                foreach (var message in messageFactory(consumeContext))
                    await SendRequest(context, consumeContext, message, serviceAddressProvider(consumeContext)).ConfigureAwait(false);

            if (asyncMessageFactory != null)
                await foreach (var message in asyncMessageFactory(consumeContext))
                    await SendRequest(context, consumeContext, message, serviceAddressProvider(consumeContext)).ConfigureAwait(false);

            await next.Execute(context).ConfigureAwait(false);
        }

        public Task Faulted<TException>(BehaviorExceptionContext<TInstance, TData, TException> context, Behavior<TInstance, TData> next)
            where TException : Exception
        {
            return next.Faulted(context);
        }

    }

}
