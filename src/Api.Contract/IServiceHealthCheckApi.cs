namespace GarageGroup.Infra;

public interface IServiceHealthCheckApi : IPingSupplier
{
    string ServiceName { get; }
}