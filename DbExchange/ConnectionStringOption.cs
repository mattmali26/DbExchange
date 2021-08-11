namespace DbExchange
{
    public class ConnectionStringOption
    {
        public string Name { get; set; }
        public string ConnectionString { get; set; }
        public DbType DbType { get; set; }
    }

    public enum DbType
    {
        MSSQL,
        POSTGRESQL
    }
}