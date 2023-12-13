using System;
using PrimeFuncPack;

namespace GarageGroup.Infra;

public static class HealthCheckHandlerDependency
{
    public static Dependency<IHealthCheckHandler> UseHealthCheckHandler(this Dependency<IServiceHealthCheckApi[]> dependency)
    {
        ArgumentNullException.ThrowIfNull(dependency);
        return dependency.Map<IHealthCheckHandler>(CreateHandler);

        static HealthCheckHandler CreateHandler(IServiceHealthCheckApi[] healthCheckApis)
            =>
            new(healthCheckApis);
    }

    public static Dependency<IHealthCheckHandler> UseHealthCheckHandler(this Dependency<IServiceHealthCheckApi> dependency)
    {
        ArgumentNullException.ThrowIfNull(dependency);
        return dependency.Map<IHealthCheckHandler>(CreateHandler);

        static HealthCheckHandler CreateHandler(IServiceHealthCheckApi healthCheckApi)
        {
            ArgumentNullException.ThrowIfNull(healthCheckApi);

            return new(
                healthCheckApis: new[]
                {
                    healthCheckApi
                });
        }
    }

    public static Dependency<IServiceHealthCheckApi> UseServiceHealthCheckApi<TApi>(this Dependency<TApi> dependency, string serviceName)
        where TApi : IPingSupplier
    {
        ArgumentNullException.ThrowIfNull(dependency);
        return dependency.Map<IServiceHealthCheckApi>(CreateApi);

        ServiceHealthCheckApi CreateApi(TApi api)
        {
            ArgumentNullException.ThrowIfNull(api);
            return new(serviceName, api);
        }
    }
}