﻿using HealthChecksTesting.WorkerService.Models.RabbitMq;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;

namespace HealthChecksTesting.WorkerService.Services.HealthChecks.RabbitMq;

public class RmqApiHealthCheck(ILogger<RmqApiHealthCheck> logger, IOptions<RabbitMqSettings> rmqSettings) : IHealthCheck, IHealthCheckService
{
    private readonly ILogger<RmqApiHealthCheck> _logger = logger;
    private readonly RabbitMqSettings _rmqSettings = rmqSettings.Value;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext? context, CancellationToken ct = default)
    {
        try
        {
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug("Starting health check for RabbitMQ API");

            var uri = new Uri(_rmqSettings.ApiHealthCheckUri!);
            using var httpClient = new HttpClient();

            var authHeader = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_rmqSettings.UserName}:{_rmqSettings.Password}")));
            httpClient.DefaultRequestHeaders.Authorization = authHeader;

            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug("RabbitMQ API settings: {uri} | {username} | {password}", uri.ToString(), _rmqSettings.UserName, _rmqSettings.Password);

            var response = await httpClient.GetAsync(uri, ct);

            var retVal = response.IsSuccessStatusCode ? HealthCheckResult.Healthy("RabbitMQ API is healthy") : HealthCheckResult.Unhealthy("RabbitMQ API is unhealthy");

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("RabbitMQ API Health Check Status: {status}", retVal.Status);

            return retVal;
        }
        catch (Exception ex)
        {
            var retVal = HealthCheckResult.Unhealthy("RabbitMQ API is unhealthy", ex);

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("RabbitMQ API Health Check Status: {status}", retVal.Status);

            return retVal;
        }
    }
}
