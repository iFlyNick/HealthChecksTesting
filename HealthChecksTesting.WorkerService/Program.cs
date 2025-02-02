using HealthChecksTesting.WorkerService.Models.Postgres;
using HealthChecksTesting.WorkerService.Models.RabbitMq;
using HealthChecksTesting.WorkerService.Services.HealthChecks.Postgres;
using HealthChecksTesting.WorkerService.Services.HealthChecks.RabbitMq;
using HealthChecksTesting.WorkerService.Workers;
using Serilog;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddLogging(s =>
        {
            s.ClearProviders()
                .AddSerilog(new LoggerConfiguration()
                    .ReadFrom.Configuration(hostContext.Configuration)
                    .CreateLogger());
        });

        services.Configure<RabbitMqSettings>(hostContext.Configuration.GetSection("RabbitMqSettings"));
        services.Configure<PostgresSettings>(hostContext.Configuration.GetSection("PostgresSettings"));

        services.AddSingleton<IRmqHealthCheck, RmqHealthCheck>();
        services.AddSingleton<IPostgresHealthCheck, PostgresHealthCheck>();

        services.AddHealthChecks()
            .AddCheck<RmqHealthCheck>("rabbitmq_health_check")
            .AddCheck<PostgresHealthCheck>("postgres_health_check");

        services.AddHostedService<BaseWorker>();
        services.AddHostedService<HealthCheckWorker>();
    })
    .Build();

await host.RunAsync();