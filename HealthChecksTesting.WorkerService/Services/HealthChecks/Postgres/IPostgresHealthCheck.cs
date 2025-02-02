using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecksTesting.WorkerService.Services.HealthChecks.Postgres;

public interface IPostgresHealthCheck
{
    Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext? context, CancellationToken ct = default);
}
