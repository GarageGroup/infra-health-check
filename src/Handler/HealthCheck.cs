using System;
using PrimeFuncPack;

namespace GarageGroup.Infra;

public static class HealthCheck
{
    public static Dependency<IServiceHealthCheckApi[]> UseEmpty()
        =>
        Dependency.Of(Array.Empty<IServiceHealthCheckApi>());

    public static Dependency<IServiceHealthCheckApi[]> UseServices(params Dependency<IServiceHealthCheckApi>[] dependencies)
    {
        if (dependencies?.Length is not > 0)
        {
            return UseEmpty();
        }

        return Dependency.From(ResolveServices);

        IServiceHealthCheckApi[] ResolveServices(IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);

            var healthCheckApis = new IServiceHealthCheckApi[dependencies.Length];

            for (var i = 0; i < dependencies.Length; i++)
            {
                healthCheckApis[i] = dependencies[i].Resolve(serviceProvider);
            }

            return healthCheckApis;
        }
    }
}