using System.Threading.Tasks;

using MassTransit;

namespace Cogito.MassTransit
{

    /// <summary>
    /// Asynchronously resolves the <see cref="IRequestToken{TRequest}"/> previously captured for the request being
    /// faulted or responded to. Returning <c>null</c> indicates that no captured request is available and the
    /// operation is skipped.
    /// </summary>
    public delegate Task<IRequestToken<TRequest>?> AsyncRequestTokenFactory<in TSaga, in TMessage, TRequest>(SagaConsumeContext<TSaga, TMessage> context)
        where TSaga : class, ISaga
        where TMessage : class;

    /// <summary>
    /// Asynchronously resolves the <see cref="IRequestToken{TRequest}"/> previously captured for the request being
    /// faulted or responded to. Returning <c>null</c> indicates that no captured request is available and the
    /// operation is skipped.
    /// </summary>
    public delegate Task<IRequestToken<TRequest>?> AsyncRequestTokenFactory<in TSaga, TRequest>(SagaConsumeContext<TSaga> context)
        where TSaga : class, ISaga;

}
