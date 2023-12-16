using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using PrimeFuncPack;

namespace GarageGroup.Infra;

public static class HealthCheckHandlerDependency
{
    private const string DefaultInfoSection = "Info";

    public static Dependency<IHealthCheckHandler> UseHealthCheckHandler(
        this Dependency<IServiceHealthCheckApi[]> dependency,
        Func<IServiceProvider, HealthCheckOption> optionResolver)
    {
        ArgumentNullException.ThrowIfNull(dependency);
        ArgumentNullException.ThrowIfNull(optionResolver);

        return dependency.Map<IHealthCheckHandler>(CreateHandler);

        HealthCheckHandler CreateHandler(IServiceProvider serviceProvider, IServiceHealthCheckApi[] healthCheckApis)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);
            return new(healthCheckApis, optionResolver.Invoke(serviceProvider));
        }
    }

    public static Dependency<IHealthCheckHandler> UseHealthCheckHandler(
        this Dependency<IServiceHealthCheckApi[]> dependency, string infoSection = DefaultInfoSection)
    {
        ArgumentNullException.ThrowIfNull(dependency);

        return dependency.Map<IHealthCheckHandler>(CreateHandler);

        HealthCheckHandler CreateHandler(IServiceProvider serviceProvider, IServiceHealthCheckApi[] healthCheckApis)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);
            return new(healthCheckApis, serviceProvider.InnerResolveOption(infoSection.OrEmpty()));
        }
    }

    public static Dependency<IHealthCheckHandler> UseHealthCheckHandler(
        this Dependency<IServiceHealthCheckApi> dependency, string infoSection = DefaultInfoSection)
    {
        ArgumentNullException.ThrowIfNull(dependency);
        return dependency.Map<IHealthCheckHandler>(CreateHandler);

        HealthCheckHandler CreateHandler(IServiceProvider serviceProvider, IServiceHealthCheckApi healthCheckApi)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);
            ArgumentNullException.ThrowIfNull(healthCheckApi);

            return new(
                healthCheckApis:
                [
                    healthCheckApi
                ],
                option: serviceProvider.InnerResolveOption(infoSection.OrEmpty()));
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

    private static HealthCheckOption InnerResolveOption(this IServiceProvider serviceProvider, string sectionName)
    {
        var section = serviceProvider.GetServiceOrAbsent<IConfiguration>().OrDefault()?.GetSection(sectionName);
        if (section is null || section.Exists() is false)
        {
            return default;
        }

        var info = new Dictionary<string, string>();

        foreach (var item in section.GetChildren())
        {
            if (string.IsNullOrEmpty(item.Key) || string.IsNullOrEmpty(item.Value))
            {
                continue;
            }

            info[item.Key] = item.Value;
        }

        return new()
        {
            Info = info
        };
    }
}