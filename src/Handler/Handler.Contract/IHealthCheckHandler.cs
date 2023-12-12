using System;

namespace GarageGroup.Infra;

public interface IHealthCheckHandler : IHandler<Unit, HealthCheckOut>
{
}