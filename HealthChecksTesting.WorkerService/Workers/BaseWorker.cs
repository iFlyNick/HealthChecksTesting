namespace HealthChecksTesting.WorkerService.Workers;

public class BaseWorker(ILogger<BaseWorker> logger) : BackgroundService
{
    private readonly ILogger<BaseWorker> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug("Worker running at: {time}", DateTimeOffset.Now);

            //simulate random work here. could be service, queue, etc.

            await Task.Delay(1000, stoppingToken);
        }
    }
}
