using HealthChecksTesting.WorkerService.Services.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecksTesting.WorkerService.Workers;

public class HealthCheckWorker(ILogger<HealthCheckWorker> logger, IHealthCheckService rmqHealthCheck, IHealthCheckService npgHealthCheck) : BackgroundService
{
    private readonly ILogger<HealthCheckWorker> _logger = logger;
    private readonly IHealthCheckService _rmqHealthCheck = rmqHealthCheck;
    private readonly IHealthCheckService _postgresHealthCheck = npgHealthCheck;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug("Health Check Worker running at: {time}", DateTimeOffset.Now);

            var rmqHealth = await _rmqHealthCheck.CheckHealthAsync(new HealthCheckContext(), stoppingToken);
            var postgresHealth = await _postgresHealthCheck.CheckHealthAsync(new HealthCheckContext(), stoppingToken);

            var rList = new List<HealthStatus>()
            {
                rmqHealth.Status,
                postgresHealth.Status
            };

            var result = 
                rList.All(s => s.Equals(HealthStatus.Healthy)) ? HealthStatus.Healthy :
                rList.All(s => s.Equals(HealthStatus.Unhealthy)) ? HealthStatus.Unhealthy 
                : HealthStatus.Degraded;

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Overall health check result: {result}", result);

            await Task.Delay(5000, stoppingToken);
        }
    }
}
