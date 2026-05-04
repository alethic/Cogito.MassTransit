using MassTransit;

namespace Cogito.MassTransit
{

    /// <summary>
    /// Resolves the <see cref="IRequestToken{TRequest}"/> previously captured for the request being faulted or
    /// responded to. Returning <c>null</c> indicates that no captured request is available and the operation is
    /// skipped.
    /// </summary>
    public delegate IRequestToken<TRequest>? RequestTokenFactory<in TSaga, in TMessage, TRequest>(SagaConsumeContext<TSaga, TMessage> context)
        where TSaga : class, ISaga
        where TMessage : class;

    /// <summary>
    /// Resolves the <see cref="IRequestToken{TRequest}"/> previously captured for the request being faulted or
    /// responded to. Returning <c>null</c> indicates that no captured request is available and the operation is
    /// skipped.
    /// </summary>
    public delegate IRequestToken<TRequest>? RequestTokenFactory<in TSaga, TRequest>(SagaConsumeContext<TSaga> context)
        where TSaga : class, ISaga;

}
