using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GarageGroup.Infra;

partial class HealthCheckHandler
{
    public async ValueTask<Result<string, Failure<HandlerFailureCode>>> HandleAsync(Unit input, CancellationToken cancellationToken)
    {
        if (healthCheckApis.Length is 0)
        {
            return BuildYaml(StatusHealthy);
        }

        var failures = new ConcurrentDictionary<int, Failure<Unit>>();
        await Parallel.ForEachAsync(Enumerable.Range(0, healthCheckApis.Length), InnerHandleAsync).ConfigureAwait(false);

        var services = new Dictionary<string, string>(healthCheckApis.Length);
        var sourceExceptions = new List<Exception>(failures.Count);

        var isHealthy = true;

        for (var i = 0; i < healthCheckApis.Length; i++)
        {
            var serviceName = healthCheckApis[i].ServiceName;

            if (failures.TryGetValue(i, out var failure) is false)
            {
                services[serviceName] = StatusHealthy;
                continue;
            }

            isHealthy = false;
            services[serviceName] = string.IsNullOrWhiteSpace(failure.FailureMessage) ? StatusUnhealthy : failure.FailureMessage;

            if (failure.SourceException is not null)
            {
                sourceExceptions.Add(failure.SourceException);
            }
        }

        if (isHealthy)
        {
            return BuildYaml(StatusHealthy, services);
        }

        return Failure.Create(
            failureCode: HandlerFailureCode.Transient,
            failureMessage: BuildYaml(StatusUnhealthy, services),
            sourceException: AggregateException(sourceExceptions));

        async ValueTask InnerHandleAsync(int index, CancellationToken cancellationToken)
        {
            try
            {
                var result = await healthCheckApis[index].PingAsync(default, cancellationToken).ConfigureAwait(false);
                if (result.IsFailure)
                {
                    _ = failures.TryAdd(index, result.FailureOrThrow());
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _ = failures.TryAdd(index, ex.ToFailure($"An unexpected exception occured: '{ex.Message}'"));
            }
        }
    }

    private static Exception? AggregateException(IReadOnlyList<Exception> exceptions)
    {
        if (exceptions.Count is 0)
        {
            return null;
        }

        if (exceptions.Count is 1)
        {
            return exceptions[0];
        }

        return new AggregateException(exceptions);
    }
}