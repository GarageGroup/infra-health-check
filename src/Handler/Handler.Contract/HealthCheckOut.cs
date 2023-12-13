using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace GarageGroup.Infra;

public sealed record class HealthCheckOut
{
    public HealthCheckOut([AllowNull] string status, [AllowNull] IReadOnlyDictionary<string, string> services)
    {
        Status = status.OrEmpty();
        Services = services?.Count is not > 0 ? null : services;
    }

    public string Status { get; }

    public IReadOnlyDictionary<string, string>? Services { get; }
}