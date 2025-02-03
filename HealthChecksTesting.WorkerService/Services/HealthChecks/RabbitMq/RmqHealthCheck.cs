using HealthChecksTesting.WorkerService.Models.RabbitMq;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;

namespace HealthChecksTesting.WorkerService.Services.HealthChecks.RabbitMq;

public class RmqHealthCheck(ILogger<RmqHealthCheck> logger, IOptions<RabbitMqSettings> rmqSettings) : IHealthCheck, IRmqHealthCheck
{
    private readonly ILogger<RmqHealthCheck> _logger = logger;
    private readonly RabbitMqSettings _rmqSettings = rmqSettings.Value;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext? context, CancellationToken ct = default)
    {
        try
        {
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug("Starting health check for RabbitMQ");

            var uri = new Uri(_rmqSettings.HealthCheckUri!);
            using var httpClient = new HttpClient();

            var authHeader = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_rmqSettings.UserName}:{_rmqSettings.Password}")));
            httpClient.DefaultRequestHeaders.Authorization = authHeader;

            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug("RabbitMQ settings: {uri} | {username} | {password}", uri.ToString(), _rmqSettings.UserName, _rmqSettings.Password);

            var response = await httpClient.GetAsync(uri, ct);

            var retVal = response.IsSuccessStatusCode ? HealthCheckResult.Healthy("RabbitMQ is healthy") : HealthCheckResult.Unhealthy("RabbitMQ is unhealthy");

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("RabbitMQ Health Check Status: {status}", retVal.Status);

            return retVal;
        }
        catch (Exception ex)
        {
            var retVal = HealthCheckResult.Unhealthy("RabbitMQ is unhealthy", ex);

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("RabbitMQ Health Check Status: {status}", retVal.Status);

            return retVal;
        }
    }
}
