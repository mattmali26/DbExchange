namespace DbExchange
{
    public class StepAction
    {
        public string ConnectionName { get; set; }
        public string[] Query { get; set; }

        public string BuildQuery()
        {
            return string.Join(" ", Query);
        }
    }
}