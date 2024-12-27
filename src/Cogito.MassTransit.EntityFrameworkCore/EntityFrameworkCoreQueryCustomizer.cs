using System.Linq;

using MassTransit;

namespace Cogito.MassTransit.EntityFrameworkCore
{

    /// <summary>
    /// Provides query customzations to a query.
    /// </summary>
    /// <typeparam name="TSaga"></typeparam>
    public interface IEntityFrameworkCoreQueryCustomizer<TSaga>
        where TSaga : SagaStateMachineInstance
    {

        /// <summary>
        /// Applies any customizations to the query.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        IQueryable<TSaga> Apply(IQueryable<TSaga> query);

    }

}
