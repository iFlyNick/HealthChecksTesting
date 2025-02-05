using RabbitMQ.Client;

namespace HealthChecksTesting.WorkerService.Services.RabbitMq;

public interface IRmqConnectionService
{
    Task<IConnection?> CreateConnection(string? vHost, CancellationToken ct = default);
}
