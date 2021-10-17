using System.Threading;
using System.Threading.Tasks;

using GreenPipes;

using MassTransit;

namespace Cogito.MassTransit.Autofac.Internal
{

    /// <summary>
    /// Proxies the implementation of <see cref="IRequestClient{TRequest}"/>.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    class ConsumeContextRequestClientProxy<TRequest> : IRequestClient<TRequest>
        where TRequest : class
    {

        readonly IRequestClient<TRequest> impl;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="bus"></param>
        public ConsumeContextRequestClientProxy(ConsumeContext context)
        {
            this.impl = context.CreateRequestClient<TRequest>(context.GetPayload<IBus>());
        }

        public RequestHandle<TRequest> Create(TRequest message, CancellationToken cancellationToken = default, RequestTimeout timeout = default)
        {
            return impl.Create(message, cancellationToken, timeout);
        }

        public RequestHandle<TRequest> Create(object values, CancellationToken cancellationToken = default, RequestTimeout timeout = default)
        {
            return impl.Create(values, cancellationToken, timeout);
        }

        public Task<Response<T>> GetResponse<T>(TRequest message, CancellationToken cancellationToken = default, RequestTimeout timeout = default) where T : class
        {
            return impl.GetResponse<T>(message, cancellationToken, timeout);
        }

        public Task<Response<T>> GetResponse<T>(TRequest message, RequestPipeConfiguratorCallback<TRequest> callback, CancellationToken cancellationToken = default, RequestTimeout timeout = default) where T : class
        {
            return impl.GetResponse<T>(message, callback, cancellationToken, timeout);
        }

        public Task<Response<T>> GetResponse<T>(object values, CancellationToken cancellationToken = default, RequestTimeout timeout = default) where T : class
        {
            return impl.GetResponse<T>(values, cancellationToken, timeout);
        }

        public Task<Response<T>> GetResponse<T>(object values, RequestPipeConfiguratorCallback<TRequest> callback, CancellationToken cancellationToken = default, RequestTimeout timeout = default) where T : class
        {
            return impl.GetResponse<T>(values, callback, cancellationToken, timeout);
        }

        public Task<Response<T1, T2>> GetResponse<T1, T2>(TRequest message, CancellationToken cancellationToken = default, RequestTimeout timeout = default)
            where T1 : class
            where T2 : class
        {
            return impl.GetResponse<T1, T2>(message, cancellationToken, timeout);
        }

        public Task<Response<T1, T2>> GetResponse<T1, T2>(TRequest message, RequestPipeConfiguratorCallback<TRequest> callback, CancellationToken cancellationToken = default, RequestTimeout timeout = default)
            where T1 : class
            where T2 : class
        {
            return impl.GetResponse<T1, T2>(message, callback, cancellationToken, timeout);
        }

        public Task<Response<T1, T2>> GetResponse<T1, T2>(object values, CancellationToken cancellationToken = default, RequestTimeout timeout = default)
            where T1 : class
            where T2 : class
        {
            return impl.GetResponse<T1, T2>(values, cancellationToken, timeout);
        }

        public Task<Response<T1, T2>> GetResponse<T1, T2>(object values, RequestPipeConfiguratorCallback<TRequest> callback, CancellationToken cancellationToken = default, RequestTimeout timeout = default)
            where T1 : class
            where T2 : class
        {
            return impl.GetResponse<T1, T2>(values, callback, cancellationToken, timeout);
        }

        public Task<Response<T1, T2, T3>> GetResponse<T1, T2, T3>(TRequest message, CancellationToken cancellationToken = default, RequestTimeout timeout = default)
            where T1 : class
            where T2 : class
            where T3 : class
        {
            return impl.GetResponse<T1, T2, T3>(message, cancellationToken, timeout);
        }

        public Task<Response<T1, T2, T3>> GetResponse<T1, T2, T3>(TRequest message, RequestPipeConfiguratorCallback<TRequest> callback, CancellationToken cancellationToken = default, RequestTimeout timeout = default)
            where T1 : class
            where T2 : class
            where T3 : class
        {
            return impl.GetResponse<T1, T2, T3>(message, callback, cancellationToken, timeout);
        }

        public Task<Response<T1, T2, T3>> GetResponse<T1, T2, T3>(object values, CancellationToken cancellationToken = default, RequestTimeout timeout = default)
            where T1 : class
            where T2 : class
            where T3 : class
        {
            return impl.GetResponse<T1, T2, T3>(values, cancellationToken, timeout);
        }

        public Task<Response<T1, T2, T3>> GetResponse<T1, T2, T3>(object values, RequestPipeConfiguratorCallback<TRequest> callback, CancellationToken cancellationToken = default, RequestTimeout timeout = default)
            where T1 : class
            where T2 : class
            where T3 : class
        {
            return impl.GetResponse<T1, T2, T3>(values, callback, cancellationToken, timeout);
        }

    }

}