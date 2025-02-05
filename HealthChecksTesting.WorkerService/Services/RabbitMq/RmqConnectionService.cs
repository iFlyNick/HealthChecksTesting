using HealthChecksTesting.WorkerService.Models.RabbitMq;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Collections.Concurrent;

namespace HealthChecksTesting.WorkerService.Services.RabbitMq;

public class RmqConnectionService(ILogger<RmqConnectionService> logger, IOptions<RabbitMqSettings> rmqSettings) : IRmqConnectionService
{
    private readonly ILogger<RmqConnectionService> _logger = logger;
    private readonly RabbitMqSettings _rmqSettings = rmqSettings.Value;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private readonly ConcurrentDictionary<string, (ConnectionFactory, IConnection)> _connectionFactories = [];

    public async Task<IConnection?> CreateConnection(string? vHost, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(vHost, nameof(vHost));

        _connectionFactories.TryGetValue(vHost, out var factory);

        var connFactory = factory.Item1 is null ? CreateConnectionFactory(vHost, ct) : factory.Item1;

        if (connFactory is null)
        {
            if (_logger.IsEnabled(LogLevel.Error))
                _logger.LogError("Failed to create connection factory for vHost: {vHost}", vHost);

            throw new Exception($"Failed to create connection factory for vHost: {vHost}");
        }

        var trackedConn = factory.Item2;
        if (trackedConn is not null && trackedConn.IsOpen)
            return trackedConn;

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug("Creating connection from factory for vHost: {vHost}", vHost);

        if (_lock.CurrentCount == 0 && _rmqSettings.LockOnConnectionCreate)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning("Connection to vHost: {vHost} is locked awaiting auto recovery or encountered race condition to create connection", vHost);
            return null;
        }

        if (_logger.IsEnabled(LogLevel.Warning))
            _logger.LogWarning("Creating new connection against connection factory to vHost {vHost}", vHost);

        var conn = await connFactory.CreateConnectionAsync(ct);

        conn.ConnectionShutdownAsync += async (sender, args) =>
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                var lockHandler = _rmqSettings.LockOnConnectionCreate ? "Starting lock state. " : "";
                _logger.LogInformation("{lockHandler}Connection to vHost: {vHost} has been shutdown. Reason: {reason}", lockHandler, vHost, args.ReplyText);
            }

            if (_rmqSettings.LockOnConnectionCreate)
                await _lock.WaitAsync();
        };

        conn.RecoverySucceededAsync += (sender, args) =>
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                var lockHandler = _rmqSettings.LockOnConnectionCreate ? "Releasing lock state. " : "";
                _logger.LogInformation("{lockHandler}Connection to vHost: {vHost} has recovered", lockHandler, vHost);
            }

            if (_lock.CurrentCount > 0 && _rmqSettings.LockOnConnectionCreate)
                _lock.Release();

            return Task.CompletedTask;
        };

        conn.ConnectionRecoveryErrorAsync += (sender, args) =>
        {

            if (_logger.IsEnabled(LogLevel.Warning))
            {
                var lockHandler = _rmqSettings.LockOnConnectionCreate ? "Releasing lock state. " : "";
                _logger.LogWarning("{lockHandler}Connection to vHost: {vHost} has failed to recover. Reason: {reason}", lockHandler, vHost, args.Exception.Message);
            }

            if (_lock.CurrentCount > 0 && _rmqSettings.LockOnConnectionCreate)
                _lock.Release();

            return Task.CompletedTask;
        };

        _connectionFactories.TryAdd(vHost, (connFactory, conn));

        return conn;
    }

    private ConnectionFactory? CreateConnectionFactory(string? vHost, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(vHost, nameof(vHost));

        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Creating new connection factory for vHost: {vHost}", vHost);

        return new ConnectionFactory
        {
            Uri = new Uri(_rmqSettings.Uri!),
            AutomaticRecoveryEnabled = true,
            VirtualHost = vHost
        };
    }
}
