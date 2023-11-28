namespace AlertHandler.Facade.Slack
{
    public class SlackConfiguration
    {
        public string ExceptionsWebHook { get; set; } = null!;
        public string ApdexScoreWebHook { get; set; } = null!;
        public string PerformanceWebHook { get; set; } = null!;
    }
}
