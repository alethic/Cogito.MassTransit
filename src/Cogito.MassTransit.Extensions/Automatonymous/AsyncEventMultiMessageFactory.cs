using System.Collections.Generic;

using Automatonymous;

namespace Cogito.MassTransit.Automatonymous
{

    public delegate IAsyncEnumerable<TMessage> AsyncEventMultiMessageFactory<in TInstance, TMessage>(EventContext<TInstance> context);

    public delegate IAsyncEnumerable<TMessage> AsyncEventMultiMessageFactory<in TInstance, in TData, TMessage>(EventContext<TInstance, TData> context);

}
