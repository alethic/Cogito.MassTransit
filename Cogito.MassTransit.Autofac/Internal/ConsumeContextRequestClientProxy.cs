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

        public RequestHandle<TRequest> Create(TRequest message, CancellationToken cancellationToken = default(CancellationToken), RequestTimeout timeout = default(RequestTimeout))
        {
            return impl.Create(message, cancellationToken, timeout);
        }

        public RequestHandle<TRequest> Create(object values, CancellationToken cancellationToken = default(CancellationToken), RequestTimeout timeout = default(RequestTimeout))
        {
            return impl.Create(values, cancellationToken, timeout);
        }

        public Task<Response<T>> GetResponse<T>(TRequest message, CancellationToken cancellationToken = default(CancellationToken), RequestTimeout timeout = default(RequestTimeout)) where T : class
        {
            return impl.GetResponse<T>(message, cancellationToken, timeout);
        }

        public Task<Response<T>> GetResponse<T>(object values, CancellationToken cancellationToken = default(CancellationToken), RequestTimeout timeout = default(RequestTimeout)) where T : class
        {
            return impl.GetResponse<T>(values, cancellationToken, timeout);
        }

        public Task<(Task<Response<T1>>, Task<Response<T2>>)> GetResponse<T1, T2>(TRequest message, CancellationToken cancellationToken = default(CancellationToken), RequestTimeout timeout = default(RequestTimeout))
            where T1 : class
            where T2 : class
        {
            return impl.GetResponse<T1, T2>(message, cancellationToken, timeout);
        }

        public Task<(Task<Response<T1>>, Task<Response<T2>>)> GetResponse<T1, T2>(object values, CancellationToken cancellationToken = default(CancellationToken), RequestTimeout timeout = default(RequestTimeout))
            where T1 : class
            where T2 : class
        {
            return impl.GetResponse<T1, T2>(values, cancellationToken, timeout);
        }

    }

}