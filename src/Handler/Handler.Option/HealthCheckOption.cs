using System.Collections.Generic;

namespace GarageGroup.Infra;

public readonly record struct HealthCheckOption
{
    public IReadOnlyDictionary<string, string>? Info { get; init; }
}