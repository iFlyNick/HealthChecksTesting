using HealthChecks.UI.Client;
using HealthChecksTesting.WorkerService.Models.Postgres;
using HealthChecksTesting.WorkerService.Models.RabbitMq;
using HealthChecksTesting.WorkerService.Services.HealthChecks;
using HealthChecksTesting.WorkerService.Services.HealthChecks.Postgres;
using HealthChecksTesting.WorkerService.Services.HealthChecks.RabbitMq;
using HealthChecksTesting.WorkerService.Workers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
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

        services.AddHealthChecks()
            .AddCheck<RmqHealthCheck>("rabbitmq_health_check", tags: ["rabbitmq"])
            .AddCheck<PostgresHealthCheck>("postgres_health_check", tags: ["postgres"]);

        services.AddHealthChecksUI()
            .AddInMemoryStorage();

        services.AddSingleton<IHealthCheckService, RmqHealthCheck>();
        services.AddSingleton<IHealthCheckService, PostgresHealthCheck>();

        services.AddHostedService<BaseWorker>();
        services.AddHostedService<HealthCheckWorker>();
    })
    .ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder.UseKestrel(o =>
        {
            o.ListenAnyIP(5000);
        })
        .Configure(app =>
        {
            app.UseRouting();
            app.UseEndpoints(e =>
            {
                e.MapHealthChecks("/health", new HealthCheckOptions
                {
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
                e.MapHealthChecksUI(o =>
                {
                    o.UIPath = "/health-ui";
                    o.ApiPath = "/health-all";
                });
            });
        });
    })
    .Build();

await host.RunAsync();