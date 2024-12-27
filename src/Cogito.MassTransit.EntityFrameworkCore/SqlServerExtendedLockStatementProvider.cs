using MassTransit.EntityFrameworkCoreIntegration;

using Microsoft.EntityFrameworkCore;

namespace Cogito.MassTransit.EntityFrameworkCore
{

    /// <summary>
    /// Customizes the lock statement to a specific column.
    /// </summary>
    public class SqlServerExtendedLockStatementProvider<TSaga> : SqlLockStatementProvider, ILockStatementProvider<TSaga>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="columnName"></param>
        public SqlServerExtendedLockStatementProvider(string columnName = "CorrelationId") :
            base("dbo", $"SELECT * FROM {{0}}.{{1}} WITH (UPDLOCK, ROWLOCK) WHERE [{columnName}] = @p0", true)
        {

        }

        public override string GetRowLockStatement<TSaga2>(DbContext context)
        {
            return base.GetRowLockStatement<TSaga2>(context);
        }

    }

}
