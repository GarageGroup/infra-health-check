namespace GarageGroup.Infra;

public interface IServiceHelthCheckApi : IPingSupplier
{
    string ServiceName { get; }
}