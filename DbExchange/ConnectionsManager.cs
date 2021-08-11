using DbExchange.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace DbExchange
{
    public class ConnectionsManager : IDisposable
    {
        private readonly ILogger<ConnectionsManager> logger;

        public IDictionary<string, IDbClient> DbConnectionList { get; private set; }

        public ConnectionsManager(ILogger<ConnectionsManager> logger, IOptions<List<ConnectionStringOption>> configuration)
        {
            this.logger = logger;
            DbConnectionList = new Dictionary<string, IDbClient>();

            var connectionStrings = configuration.Value;

            foreach (var connectionString in connectionStrings)
            {
                switch (connectionString.DbType)
                {
                    case DbType.MSSQL:
                        DbConnectionList.Add(connectionString.Name, new SqlDbClient(connectionString.ConnectionString));
                        break;

                    case DbType.POSTGRESQL:
                        DbConnectionList.Add(connectionString.Name, new PostgreDbClient(connectionString.ConnectionString));
                        break;
                }
            }

            Connect();
        }

        private void Connect()
        {
            foreach (var connection in DbConnectionList)
            {
                connection.Value.OpenConnection();
            }
        }

        public void Dispose()
        {
            foreach (var connection in DbConnectionList)
            {
                connection.Value.CloseConnection();
            }
        }
    }
}