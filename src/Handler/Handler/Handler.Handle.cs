using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GarageGroup.Infra;

partial class HealthCheckHandler
{
    public async ValueTask<Result<HealthCheckOut, Failure<HandlerFailureCode>>> HandleAsync(Unit input, CancellationToken cancellationToken)
    {
        if (healthCheckApis.Length is 0)
        {
            return EmptyHealthCheckOutput;
        }

        var failures = new ConcurrentBag<(int Index, Failure<Unit> Failure)>();
        await Parallel.ForEachAsync(Enumerable.Range(0, healthCheckApis.Length), InnerHandleAsync).ConfigureAwait(false);

        if (failures.IsEmpty)
        {
            return new HealthCheckOut(
                status: StatusHealthy,
                services: healthCheckApis.ToDictionary(GetServiceName, GetStatusHealthy));
        }

        var failure = failures.OrderBy(GetIndex).First();
        var serviceName = healthCheckApis[failure.Index].ServiceName;

        return Failure.Create(
            failureCode: HandlerFailureCode.Transient,
            failureMessage: $"\"{serviceName}\": \"{failure.Failure.FailureMessage}\"",
            sourceException: failure.Failure.SourceException);

        async ValueTask InnerHandleAsync(int index, CancellationToken cancellationToken)
        {
            try
            {
                var result = await healthCheckApis[index].PingAsync(default, cancellationToken).ConfigureAwait(false);
                if (result.IsFailure)
                {
                    failures.Add((index, result.FailureOrThrow()));
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                failures.Add((index, ex.ToFailure("An unexpected exception occured")));
            }
        }

        static int GetIndex((int Index, Failure<Unit>) item)
            =>
            item.Index;

        static string GetServiceName(IServiceHealthCheckApi api)
            =>
            api.ServiceName;

        static string GetStatusHealthy(IServiceHealthCheckApi _)
            =>
            StatusHealthy;
    }
}