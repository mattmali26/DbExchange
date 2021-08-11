using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Text.Json;
using System.Threading;

namespace DbExchange
{
    public class WorkflowManager
    {
        private Workflow Workflow;
        private string ConfigFileJsonFullPath { get; set; }

        public ConnectionsManager ConnectionManager { get; set; }

        public WorkflowManager(IOptions<FileQuerySettings> configuration, ConnectionsManager connectionsManager)
        {
            ConnectionManager = connectionsManager;

            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var configFileJson = configuration.Value.FileName;
            ConfigFileJsonFullPath = Path.Combine(baseDirectory, configFileJson);

            using (var file = new FileInfo(ConfigFileJsonFullPath).OpenText())
            {
                var fileContent = file.ReadToEnd();
                Workflow = JsonSerializer.Deserialize<Workflow>(fileContent);
            }

            Thread.Sleep(500);
        }

        public void SaveWorkflowJSON()
        {
            lock (typeof(WorkflowManager))
            {
                using (var file = File.CreateText(ConfigFileJsonFullPath))
                {
                    Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                    serializer.Formatting = Newtonsoft.Json.Formatting.Indented;
                    serializer.Serialize(file, Workflow);
                    Thread.Sleep(500);
                }
            }
        }

        public void Process()
        {
            Workflow.Process(this);
        }
    }
}