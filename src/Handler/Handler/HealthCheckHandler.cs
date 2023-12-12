using System;
using System.Diagnostics.CodeAnalysis;

namespace GarageGroup.Infra;

internal sealed partial class HealthCheckHandler : IHealthCheckHandler
{
    private const string StatusHealthy = "Healthy";

    public static readonly HealthCheckHandler Empty;

    private static readonly HealthCheckOut EmptyHealthCheckOutput;

    static HealthCheckHandler()
    {
        Empty = new(default);
        EmptyHealthCheckOutput = new(StatusHealthy, default);
    }

    private readonly IServiceHealthCheckApi[] healthCheckApis;

    internal HealthCheckHandler([AllowNull] IServiceHealthCheckApi[] healthCheckApis)
        =>
        this.healthCheckApis = healthCheckApis ?? Array.Empty<IServiceHealthCheckApi>();
}