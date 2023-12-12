using System;
using System.Threading;
using System.Threading.Tasks;

namespace GarageGroup.Infra;

internal sealed class ServiceHelthCheckApi : IServiceHelthCheckApi
{
    private readonly IPingSupplier pingSupplier;

    internal ServiceHelthCheckApi(string serviceName, IPingSupplier pingSupplier)
    {
        ServiceName = serviceName.OrEmpty();
        this.pingSupplier = pingSupplier;
    }

    public string ServiceName { get; private init; }

    public ValueTask<Result<Unit, Failure<Unit>>> PingAsync(Unit input, CancellationToken cancellationToken)
        =>
        pingSupplier.PingAsync(input, cancellationToken);
}