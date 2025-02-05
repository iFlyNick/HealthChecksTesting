namespace HealthChecksTesting.WorkerService.Models.RabbitMq;

public class RabbitMqSettings
{
    public string? ApiHealthCheckUri { get; set; }
    public string? Uri { get; set; }
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public bool LockOnConnectionCreate { get; set; } = true;
}
