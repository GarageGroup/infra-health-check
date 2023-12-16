using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace GarageGroup.Infra;

internal sealed partial class HealthCheckHandler : IHealthCheckHandler
{
    private const string StatusHealthy = "Healthy";

    private const string StatusUnhealthy = "Unhealthy";

    private static readonly ISerializer YamlSerializer;

    static HealthCheckHandler()
        =>
         YamlSerializer = new SerializerBuilder().WithNamingConvention(PascalCaseNamingConvention.Instance).Build();

    private readonly IServiceHealthCheckApi[] healthCheckApis;

    private readonly HealthCheckOption option;

    internal HealthCheckHandler([AllowNull] IServiceHealthCheckApi[] healthCheckApis, HealthCheckOption option)
    {
        this.healthCheckApis = healthCheckApis ?? [];
        this.option = option;
    }

    private string BuildYaml(string status, [AllowNull] IReadOnlyDictionary<string, string>? services = default)
    {
        var yaml = new HealthCheckYamlOut(option.Info, status, services);
        return YamlSerializer.Serialize(yaml);
    }
}