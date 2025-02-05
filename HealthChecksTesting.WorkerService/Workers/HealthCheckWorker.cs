using HealthChecksTesting.WorkerService.Services.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecksTesting.WorkerService.Workers;

public class HealthCheckWorker(ILogger<HealthCheckWorker> logger, IEnumerable<IHealthCheckService> hcServices) : BackgroundService
{
    private readonly ILogger<HealthCheckWorker> _logger = logger;
    private readonly IEnumerable<IHealthCheckService> _hcServices = hcServices;

    private bool _firstCheck;

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        //need to force wait on the first check against rabbitmq to avoid a race condition with the base IHealthCheck since they are independent checks, but utilizing the same connection
        if (_firstCheck)
        {
            var firstDelay = 5000;
            _firstCheck = false;

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Waiting {time}ms for first worker check to avoid race condition for rabbitmq connection creation", firstDelay);

            await Task.Delay(5000, ct);

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("First worker check delay complete");
        }

        while (!ct.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug("Health Check Worker running at: {time}", DateTimeOffset.Now);

            var rList = new List<HealthStatus>();

            foreach (var hcService in _hcServices)
            {
                var hc = await hcService.CheckHealthAsync(new HealthCheckContext(), ct);
                rList.Add(hc.Status);
            }

            var result = 
                rList.All(s => s.Equals(HealthStatus.Healthy)) ? HealthStatus.Healthy :
                rList.All(s => s.Equals(HealthStatus.Unhealthy)) ? HealthStatus.Unhealthy 
                : HealthStatus.Degraded;

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Overall health check result: {result}", result);

            await Task.Delay(5000, ct);
        }
    }
}
