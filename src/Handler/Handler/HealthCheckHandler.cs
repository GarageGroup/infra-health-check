using System;
using System.Diagnostics.CodeAnalysis;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace GarageGroup.Infra;

internal sealed partial class HealthCheckHandler : IHealthCheckHandler
{
    private const string StatusHealthy = "Healthy";

    private const string StatusUnhealthy = "Unhealthy";

    public static readonly HealthCheckHandler Empty;

    private static readonly Lazy<string> EmptyOutputLazy;

    private static readonly ISerializer YamlSerializer;

    static HealthCheckHandler()
    {
        Empty = new(default);

        YamlSerializer = new SerializerBuilder().WithNamingConvention(PascalCaseNamingConvention.Instance).Build();
        EmptyOutputLazy = new(BuildEmptyOutput);
    }

    private readonly IServiceHealthCheckApi[] healthCheckApis;

    internal HealthCheckHandler([AllowNull] IServiceHealthCheckApi[] healthCheckApis)
        =>
        this.healthCheckApis = healthCheckApis ?? [];

    private static string BuildEmptyOutput()
        =>
        Serialize(
            yaml: new(StatusHealthy, default));

    private static string Serialize(HealthCheckYamlOut yaml)
        =>
        YamlSerializer.Serialize(yaml);
}