using System.Collections.Generic;

using Automatonymous;

namespace Cogito.MassTransit.Automatonymous
{

    public delegate IEnumerable<TMessage> EventMultiMessageFactory<in TInstance, out TMessage>(
        ConsumeEventContext<TInstance> context);

    public delegate IEnumerable<TMessage> EventMultiMessageFactory<in TInstance, in TData, out TMessage>(
        ConsumeEventContext<TInstance, TData> context)
        where TData : class;

}
