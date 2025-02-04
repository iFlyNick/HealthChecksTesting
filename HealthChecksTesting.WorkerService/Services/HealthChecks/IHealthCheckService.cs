using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecksTesting.WorkerService.Services.HealthChecks;

public interface IHealthCheckService
{
    Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext? context, CancellationToken ct = default);
}
