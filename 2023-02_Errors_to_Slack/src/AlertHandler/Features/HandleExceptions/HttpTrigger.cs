using System.Net;
using System.Text.Json;
using AlertHandler.Facade.ApplicationInsights;
using AlertHandler.Facade.Slack;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace AlertHandler.Features.HandleExceptions
{
    public class HttpTrigger
    {
        private readonly ISlackApi _slackApi;
        private readonly ILogger _logger;

        public HttpTrigger(ILogger<HttpTrigger> logger, ISlackApi slackApi)
        {
            _logger = logger;
            _slackApi = slackApi;
        }

        [Function("HandleExceptions")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "exceptions")] HttpRequestData req)
        {
            var alert = JsonSerializer.Deserialize<LogAlert>(await new StreamReader(req.Body).ReadToEndAsync())!;

            var serializedAlert = JsonSerializer.Serialize(alert);
            _logger.LogInformation("Exception alert: {SerializedAlert}", serializedAlert);

            await _slackApi.Send(BuildTextHandleExceptions(alert));

            return req.CreateResponse(HttpStatusCode.NoContent);
        }
        [Function("ApdexScore")]
        public async Task<HttpResponseData> RunWarnings([HttpTrigger(AuthorizationLevel.Function, "post", Route = "apdex")] HttpRequestData req)
        {
            var alert = JsonSerializer.Deserialize<LogAlert>(await new StreamReader(req.Body).ReadToEndAsync())!;

            _logger.LogInformation("Low Apdex: {@Alert}", alert);

            await _slackApi.Send(BuildTextApdexScore(alert));

            return req.CreateResponse(HttpStatusCode.NoContent);
        }

        [Function("Performance")]
        public async Task<HttpResponseData> RunPerformance([HttpTrigger(AuthorizationLevel.Function, "post", Route = "performance")] HttpRequestData req)
        {
            var alert = JsonSerializer.Deserialize<LogAlert>(await new StreamReader(req.Body).ReadToEndAsync())!;

            _logger.LogInformation("Performance alert: {@Alert}", alert);

            await _slackApi.Send(BuildTextPerformance(alert));

            return req.CreateResponse(HttpStatusCode.NoContent);
        }

        private static string BuildTextHandleExceptions(LogAlert alert)
        {
            string? exceptionMessage = string.IsNullOrWhiteSpace(alert?.ExceptionMessage()) ? "" : alert?.ExceptionMessage();
            return $"`{alert?.FiredDateTime()}` - {alert?.AlertRule()} - :boom: New errors ({alert?.GetErrorCount()}): <{alert?.LinkToSearchResults()}|{exceptionMessage}> from {alert?.CloudRoleName()}";
        }
        private static string BuildTextApdexScore(LogAlert alert)
        {
            return $"`{alert!.FiredDateTime()}` - {alert!.AlertRule()} - :sloth: Low Apdex Score ≤ {alert!.Threshold()} (<{alert!.LinkToSearchResults()}|{alert!.MetricValue()}>)";
        }
        private static string BuildTextPerformance(LogAlert alert)
        {
            return $"`{alert!.FiredDateTime()}` - {alert!.AlertRule()} - :sloth: Server Response Time high - Dynamic Threshold";
        }
    }
}