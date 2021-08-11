using DbExchange.Abstractions;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace DbExchange
{
    public class SqlDbClient : IDbClient
    {
        private readonly SqlConnection sqlConnection;

        public SqlDbClient(string connectionString)
        {
            sqlConnection = new SqlConnection(connectionString);
        }

        public void CloseConnection()
        {
            sqlConnection.Close();
        }

        public void OpenConnection()
        {
            sqlConnection.Open();
        }

        public DataTable ReadDataFromQuery(string query, string processName, DateTime? filterDate)
        {
            var dataTable = new DataTable();
            var selectCommand = new SqlCommand(query, sqlConnection);

            if (filterDate.HasValue)
            {
                selectCommand.Parameters.Add(new SqlParameter("@fromDataImage", filterDate.Value));
            }

            new SqlDataAdapter(selectCommand).Fill(dataTable);

            return dataTable;
        }

        public bool ShouldInsertData(string query, DataRow fetchDataRow)
        {
            var sqlCommand = new SqlCommand(query, sqlConnection);

            var parameters = ExtractSqlParametersFromQuery(query);
            foreach (var param in parameters)
            {
                var columnName = param.Replace("@", "");
                var columnValue = fetchDataRow[columnName];
                sqlCommand.Parameters.AddWithValue(param, columnValue);
            }

            return Convert.ToInt64(sqlCommand.ExecuteScalar()) == 0L;
        }

        public void ExecuteQuery(string query, DataRow fetchDataRow)
        {
            var sqlCommand = new SqlCommand(query, sqlConnection);

            var parameters = ExtractSqlParametersFromQuery(query);
            foreach (var param in parameters.Where(x => !x.EndsWith("__")))
            {
                var columnName = param.Replace("@", "");
                var columnValue = fetchDataRow[columnName];
                sqlCommand.Parameters.AddWithValue(param, columnValue);
            }

            sqlCommand.ExecuteNonQuery();
        }

        private string[] ExtractSqlParametersFromQuery(string query)
        {
            Regex r = new Regex(@"(?<Parameter>@\w*)", RegexOptions.Compiled);
            string[] parameters = r.Matches(query).Cast<Match>().Select<Match, string>(x => x.Value.ToLower()).Distinct<string>().ToArray<string>();
            return parameters;
        }
    }
}