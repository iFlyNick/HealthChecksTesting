using HealthChecksTesting.WorkerService.Services.HealthChecks.Postgres;
using HealthChecksTesting.WorkerService.Services.HealthChecks.RabbitMq;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecksTesting.WorkerService.Workers;

public class HealthCheckWorker(ILogger<HealthCheckWorker> logger, IRmqHealthCheck rmqHealthCheck, IPostgresHealthCheck npgHealthCheck) : BackgroundService
{
    private readonly ILogger<HealthCheckWorker> _logger = logger;
    private readonly IRmqHealthCheck _rmqHealthCheck = rmqHealthCheck;
    private readonly IPostgresHealthCheck _postgresHealthCheck = npgHealthCheck;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug("Health Check Worker running at: {time}", DateTimeOffset.Now);

            var rmqHealth = Task.Run(async() => await _rmqHealthCheck.CheckHealthAsync(null, stoppingToken));
            var postgresHealth = Task.Run(async () => await _postgresHealthCheck.CheckHealthAsync(null, stoppingToken));

            await Task.WhenAll(rmqHealth, postgresHealth);

            var rList = new List<HealthStatus>()
            {
                rmqHealth.Result.Status,
                postgresHealth.Result.Status
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
