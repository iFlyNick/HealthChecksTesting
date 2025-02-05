using HealthChecksTesting.WorkerService.Services.RabbitMq;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecksTesting.WorkerService.Services.HealthChecks.RabbitMq;

public class RmqConnectionHealthCheck(ILogger<RmqConnectionHealthCheck> logger, IRmqConnectionService rmqConnectionService) : IHealthCheck, IHealthCheckService
{
    private readonly ILogger<RmqConnectionHealthCheck> _logger = logger;
    private readonly IRmqConnectionService _rmqConnectionService = rmqConnectionService;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext? context, CancellationToken ct = default)
    {
        try
        {
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug("Starting health check for RabbitMQ Connection");

            var conn = await _rmqConnectionService.CreateConnection("/", ct);

            ArgumentNullException.ThrowIfNull(conn, nameof(conn));

            using var channel = await conn.CreateChannelAsync(cancellationToken: ct);

            var retVal = channel.IsOpen ? HealthCheckResult.Healthy("RabbitMQ Connection is healthy") : HealthCheckResult.Unhealthy("RabbitMQ Connection is unhealthy");

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("RabbitMQ Connection Health Check Status: {status}", retVal.Status);
            
            return retVal;
        }
        catch (Exception ex)
        {
            var retVal = HealthCheckResult.Unhealthy("RabbitMQ Connection is unhealthy", ex);

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("RabbitMQ Connection Health Check Status: {status}", retVal.Status);

            return retVal;
        }
    }
}
