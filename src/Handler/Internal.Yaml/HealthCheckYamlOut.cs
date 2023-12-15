using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace GarageGroup.Infra;

internal sealed record class HealthCheckYamlOut
{
    public HealthCheckYamlOut(string status, [AllowNull] IReadOnlyDictionary<string, string> services)
    {
        Status = status.OrEmpty();
        Services = services?.Count is not > 0 ? null : services;
    }

    public string Status { get; }

    public IReadOnlyDictionary<string, string>? Services { get; }
}