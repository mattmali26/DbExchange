using System;
using System.Data;

namespace DbExchange.Abstractions
{
    public interface IDbClient
    {
        void OpenConnection();

        void CloseConnection();

        DataTable ReadDataFromQuery(string query, string processName, DateTime? filterDate);

        bool ShouldInsertData(string query, DataRow fetchDataRow);

        void ExecuteQuery(string query, DataRow fetchDataRow);
    }
}