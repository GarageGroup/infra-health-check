using System;
using PrimeFuncPack;

namespace GarageGroup.Infra;

public static class HealthCheckHandlerDependency
{
    public static Dependency<IHealthCheckHandler> UseHealthCheckHandler(this Dependency<IServiceHelthCheckApi[]> dependency)
    {
        ArgumentNullException.ThrowIfNull(dependency);
        return dependency.Map<IHealthCheckHandler>(CreateHandler);

        static HealthCheckHandler CreateHandler(IServiceHelthCheckApi[] healthCheckApis)
            =>
            new(healthCheckApis);
    }

    public static Dependency<IServiceHelthCheckApi> UseServiceHelthCheckApi<TApi>(this Dependency<TApi> dependency, string serviceName)
        where TApi : IPingSupplier
    {
        ArgumentNullException.ThrowIfNull(dependency);
        return dependency.Map<IServiceHelthCheckApi>(CreateApi);

        ServiceHelthCheckApi CreateApi(TApi api)
        {
            ArgumentNullException.ThrowIfNull(api);
            return new(serviceName, api);
        }
    }
}