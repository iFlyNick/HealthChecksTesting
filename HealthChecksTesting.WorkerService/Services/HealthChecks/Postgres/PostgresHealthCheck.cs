using Dapper;
using HealthChecksTesting.WorkerService.Models.Postgres;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Npgsql;

namespace HealthChecksTesting.WorkerService.Services.HealthChecks.Postgres;

public class PostgresHealthCheck(ILogger<PostgresHealthCheck> logger, IOptions<PostgresSettings> dbSettings) : IHealthCheck, IHealthCheckService
{
    private readonly ILogger<PostgresHealthCheck> _logger = logger;
    private readonly PostgresSettings _dbSettings = dbSettings.Value;

    private const string _sqlCheck = "SELECT 1";

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext? context, CancellationToken ct = default)
    {
        try
        {
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug("Starting health check for Postgres");

            using var connection = new NpgsqlConnection(_dbSettings.ConnectionString!);

            var cmd = new CommandDefinition(_sqlCheck, cancellationToken: ct);

            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug("Postgres Health Check SQL: {sql} | {conn}", _sqlCheck, _dbSettings.ConnectionString);

            var resp = await connection.QueryAsync<int>(cmd);

            var retVal = resp.FirstOrDefault() == 1 ? HealthCheckResult.Healthy("Postgres is healthy") : HealthCheckResult.Unhealthy("Postgres is unhealthy");

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Postgres Health Check Status: {status}", retVal.Status);

            return retVal;
        }
        catch (Exception ex)
        {
            var retVal = HealthCheckResult.Unhealthy("Postgres is unhealthy", ex);

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Postgres Health Check Status: {status}", retVal.Status);
            
            return retVal;
        }
    }
}
