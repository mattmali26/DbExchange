using System.Collections.Generic;
using System.Threading;

namespace DbExchange
{
    public class Workflow
    {
        public List<Step> Steps { get; set; }
        public double FetchDataDeltaSeconds { get; set; }

        public void Process(WorkflowManager workflowManager)
        {
            foreach (var step in Steps)
            {
                step.Process(workflowManager, FetchDataDeltaSeconds);
                Thread.Sleep(500);
            }
        }
    }
}