using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecksTesting.WorkerService.Services.HealthChecks.RabbitMq;

public interface IRmqHealthCheck
{
    Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext? context, CancellationToken ct = default);
}
