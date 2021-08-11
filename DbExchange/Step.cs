using System;
using System.Collections.Generic;
using System.Data;

namespace DbExchange
{
    public class Step
    {
        //private readonly ILogger<Step> logger;
        //private readonly IServiceProvider serviceProvider;

        public DateTime FromDataImage { get; set; }
        public string DataImageUpdateColumn { get; set; }
        public string StepName { get; set; }
        public StepAction Fetch { get; set; }
        public StepAction Check { get; set; }
        public List<StepAction> Update { get; set; }
        public List<StepAction> Create { get; set; }

        private ConnectionsManager connectionsManager;

        private DateTime MinDate { get; set; }
        private DateTime MaxDate { get; set; }

        public Step()
        {
        }

        //public Step(IServiceProvider serviceProvider)
        //{
        //    this.serviceProvider = serviceProvider;
        //}

        public void Process(WorkflowManager workflowManager, double fetchDataDeltaSeconds)
        {
            //using var scope = serviceProvider.CreateScope();
            //var logger = scope.ServiceProvider.GetRequiredService<ILogger<Step>>();
            //logger.LogInformation($"Process step: {StepName}");

            connectionsManager = workflowManager.ConnectionManager;

            var dbClient = connectionsManager.DbConnectionList[Fetch.ConnectionName];
            var query = Fetch.BuildQuery();

            DateTime? filterDate = null;
            if (!string.IsNullOrWhiteSpace(DataImageUpdateColumn))
            {
                filterDate = FromDataImage.AddSeconds(fetchDataDeltaSeconds);
            }

            var fetchData = dbClient.ReadDataFromQuery(query, "", filterDate);

            // If there are rows to process
            if (fetchData.Rows.Count > 0)
            {
                if (!string.IsNullOrWhiteSpace(DataImageUpdateColumn))
                {
                    // Get min and max date from dataTable
                    MinDate = (DateTime)fetchData.Compute("MIN(" + DataImageUpdateColumn + ")", null);
                    MaxDate = (DateTime)fetchData.Compute("MAX(" + DataImageUpdateColumn + ")", null);
                }

                foreach (DataRow row in fetchData.Rows)
                {
                    if (ShouldInsertData(row))
                    {
                        CreateRow(row);
                    }
                    else
                    {
                        UpdateRow(row);
                    }
                }

                FromDataImage = string.IsNullOrWhiteSpace(DataImageUpdateColumn) ? DateTime.Now : MaxDate;
                workflowManager.SaveWorkflowJSON();
            }
        }

        private bool ShouldInsertData(DataRow fetchDataRow)
        {
            string checkQuery = Check.BuildQuery();
            var dbClient = connectionsManager.DbConnectionList[Check.ConnectionName];

            var shouldInsert = dbClient.ShouldInsertData(checkQuery, fetchDataRow);
            return shouldInsert;
        }

        private void CreateRow(DataRow fetchDataRow)
        {
            foreach (var step in Create)
            {
                var query = step.BuildQuery();
                var dbClient = connectionsManager.DbConnectionList[step.ConnectionName];

                dbClient.ExecuteQuery(query, fetchDataRow);
            }
        }

        private void UpdateRow(DataRow fetchDataRow)
        {
            foreach (var step in Update)
            {
                var query = step.BuildQuery();
                var dbClient = connectionsManager.DbConnectionList[step.ConnectionName];

                dbClient.ExecuteQuery(query, fetchDataRow);
            }
        }
    }
}