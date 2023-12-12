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

    private readonly IServiceHelthCheckApi[] helthCheckApis;

    internal HealthCheckHandler([AllowNull] IServiceHelthCheckApi[] helthCheckApis)
        =>
        this.helthCheckApis = helthCheckApis ?? Array.Empty<IServiceHelthCheckApi>();
}