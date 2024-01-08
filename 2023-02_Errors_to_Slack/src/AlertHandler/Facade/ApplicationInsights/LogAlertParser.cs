namespace AlertHandler.Facade.ApplicationInsights
{
    public static class LogAlertParser
    {
        public static string LinkToSearchResults(this LogAlert alert)
        {
            var allOf = alert.data.alertContext.condition.allOf[0];
            return allOf.linkToSearchResultsUI.Replace("\n", "");
        }

        public static string? ExceptionMessage(this LogAlert alert)
        {
            var dimension = alert!.GetDimension("outerMessage");

            // If outerMessage is null or empty, return null
            if (string.IsNullOrWhiteSpace(dimension))
                return null;

            return dimension;
        }

        public static string? CloudRoleName(this LogAlert alert)
            => alert!.GetDimension("cloud_RoleName");

        public static int GetErrorCount(this LogAlert alert)
            => Convert.ToInt32(alert!.GetDimension("ErrorCount"));

        private static string? GetDimension(this LogAlert alert, string dimensionName)
            => alert!.data.alertContext.condition.allOf[0].dimensions.FirstOrDefault(x => x.name == dimensionName)?.value;

        public static bool LowApdex(this LogAlert alert)
        {
            var metricMeasureColumn = alert!.data.alertContext.condition.allOf[0].metricMeasureColumn;
            return metricMeasureColumn == "AverageApdexScore";
        }

        public static string? Threshold(this LogAlert alert)
        {
            if (alert.LowApdex())
            {
                var threshold = alert!.data.alertContext.condition.allOf[0].threshold.ToString();
                return threshold;
            }

            return null;
        }

        public static string? MetricValue(this LogAlert alert)
        {
            if (alert.LowApdex())
            {
                var metricValue = alert!.data.alertContext.condition.allOf[0].metricValue.ToString();
                return metricValue;
            }

            return null;
        }

        public static string? AlertRule(this LogAlert alert)
        {
            return alert!.data.essentials.alertRule;
        }

        public static DateTime FiredDateTime(this LogAlert alert)
        {
            return alert!.data.essentials.firedDateTime;
        }
    }
}
