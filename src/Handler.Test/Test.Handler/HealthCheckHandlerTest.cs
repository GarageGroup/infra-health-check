using System;
using System.Collections.Generic;
using System.Threading;
using Moq;
using Xunit;

namespace GarageGroup.Infra.HealthCheck.Handler.Test;

public static partial class HealthCheckHandlerTest
{
    public static TheoryData<HealthCheckOption, string> WithoutServicesCases
        =>
        [
            new(
                default,
                """
                Info: 
                Status: Healthy
                Services: 
                """),
            new(
                new()
                {
                    Info = new Dictionary<string, string>
                    {
                        ["Service"] = "HealthCheck",
                        ["Version"] = "1.0.0"
                    }
                },
                """
                Info:
                  Service: HealthCheck
                  Version: 1.0.0
                Status: Healthy
                Services: 
                """)
        ];

    

    public static TheoryData<HealthCheckOption, string> ServiceFailureCases
        =>
        [
            new(
                default,
                """
                Info: 
                Status: Unhealthy
                Services:
                  Inventory: Healthy
                  Billing: Dependency timeout
                """),
            new(
                new()
                {
                    Info = new Dictionary<string, string>
                    {
                        ["Service"] = "HealthCheck",
                        ["Environment"] = "Test"
                    }
                },
                """
                Info:
                  Service: HealthCheck
                  Environment: Test
                Status: Unhealthy
                Services:
                  Inventory: Healthy
                  Billing: Dependency timeout
                """)
        ];

    private static Mock<IServiceHealthCheckApi> BuildHealthCheckApiMock(
        string serviceName,
        in Result<Unit, Failure<Unit>> pingResult)
    {
        var healthCheckApiMock = new Mock<IServiceHealthCheckApi>();

        healthCheckApiMock.SetupGet(static api => api.ServiceName).Returns(serviceName);

        healthCheckApiMock.Setup(
            static api => api.PingAsync(It.IsAny<Unit>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(pingResult);

        return healthCheckApiMock;
    }

    private static Mock<IServiceHealthCheckApi> BuildHealthCheckApiMock(
        string serviceName, Exception pingException)
    {
        var healthCheckApiMock = new Mock<IServiceHealthCheckApi>();

        healthCheckApiMock.SetupGet(static api => api.ServiceName).Returns(serviceName);

        healthCheckApiMock.Setup(
            static api => api.PingAsync(It.IsAny<Unit>(), It.IsAny<CancellationToken>()))
        .Throws(pingException);

        return healthCheckApiMock;
    }

    private static string NormalizeExpectedYaml(string yaml)
    {
        var normalized = yaml.TrimStart('\r', '\n').ReplaceLineEndings("\n");
        return normalized.EndsWith('\n') ? normalized : normalized + "\n";
    }
}
