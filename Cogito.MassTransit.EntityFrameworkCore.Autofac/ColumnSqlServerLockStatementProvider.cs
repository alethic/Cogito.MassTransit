
using MassTransit.EntityFrameworkCoreIntegration;

using Microsoft.EntityFrameworkCore;

namespace Cogito.MassTransit.EntityFrameworkCore.Autofac
{

    /// <summary>
    /// Customizes the lock statement to a specific column.
    /// </summary>
    class ColumnSqlServerLockStatementProvider<TSaga> : SqlLockStatementProvider
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="columnName"></param>
        public ColumnSqlServerLockStatementProvider(string columnName = "CorrelationId") :
            base("dbo", $"SELECT * FROM {{0}}.{{1}} WITH (UPDLOCK, ROWLOCK) WHERE [{columnName}] = @p0", true)
        {

        }

        public override string GetRowLockStatement<TSaga2>(DbContext context)
        {
            return base.GetRowLockStatement<TSaga2>(context);
        }

    }

}
