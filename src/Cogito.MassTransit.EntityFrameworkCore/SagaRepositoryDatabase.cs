namespace Cogito.MassTransit.EntityFrameworkCore
{

    /// <summary>
    /// The database a saga repository locks instances in.
    /// </summary>
    public enum SagaRepositoryDatabase
    {

        SqlServer,
        Postgres,
        MySql,
        Sqlite,
        Oracle,

    }

}
