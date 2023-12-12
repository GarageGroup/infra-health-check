using System;
using PrimeFuncPack;

namespace GarageGroup.Infra;

public static class HealthCheck
{
    public static Dependency<IServiceHelthCheckApi[]> UseEmpty()
        =>
        Dependency.Of(Array.Empty<IServiceHelthCheckApi>());

    public static Dependency<IServiceHelthCheckApi[]> UseServices(params Dependency<IServiceHelthCheckApi>[] dependencies)
    {
        if (dependencies?.Length is not > 0)
        {
            return UseEmpty();
        }

        return Dependency.From(ResolveServices);

        IServiceHelthCheckApi[] ResolveServices(IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);

            var healthCheckApis = new IServiceHelthCheckApi[dependencies.Length];

            for (var i = 0; i < dependencies.Length; i++)
            {
                healthCheckApis[i] = dependencies[i].Resolve(serviceProvider);
            }

            return healthCheckApis;
        }
    }
}