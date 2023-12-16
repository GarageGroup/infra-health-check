using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace GarageGroup.Infra;

internal sealed record class HealthCheckYamlOut
{
    public HealthCheckYamlOut(
        [AllowNull] IReadOnlyDictionary<string, string> info,
        string status,
        [AllowNull] IReadOnlyDictionary<string, string> services)
    {
        Info = info?.Count is not > 0 ? null : info;
        Status = status.OrEmpty();
        Services = services?.Count is not > 0 ? null : services;
    }

    public IReadOnlyDictionary<string, string>? Info { get; }

    public string Status { get; }

    public IReadOnlyDictionary<string, string>? Services { get; }
}