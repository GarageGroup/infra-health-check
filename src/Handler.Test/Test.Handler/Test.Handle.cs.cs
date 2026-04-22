using System;
using System.Threading.Tasks;
using Xunit;

namespace GarageGroup.Infra.HealthCheck.Handler.Test;

public static partial class HealthCheckHandlerTest
{
    [Theory]
    [MemberData(nameof(WithoutServicesCases))]
    public static async Task HandleAsync_WithoutServices_ReturnsHealthyYaml(
        HealthCheckOption option, string expectedYaml)
    {
        var handler = new HealthCheckHandler(
            healthCheckApis: [],
            option: option);

        var actual = await handler.HandleAsync(default, TestContext.Current.CancellationToken);
        var expected = NormalizeExpectedYaml(expectedYaml);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public static async Task HandleAsync_WhenAllServicesAreHealthy_ReturnsHealthyYamlWithServices()
    {
        var firstApiMock = BuildHealthCheckApiMock(
            serviceName: "Inventory",
            pingResult: default(Unit));

        var secondApiMock = BuildHealthCheckApiMock(
            serviceName: "Billing",
            pingResult: default(Unit));

        var handler = new HealthCheckHandler(
            healthCheckApis:
            [
                firstApiMock.Object,
                secondApiMock.Object
            ],
            option: default);

        var actual = await handler.HandleAsync(default, TestContext.Current.CancellationToken);

        var expected = NormalizeExpectedYaml(
            """
            Info: 
            Status: Healthy
            Services:
              Inventory: Healthy
              Billing: Healthy
            """);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [MemberData(nameof(ServiceFailureCases))]
    public static async Task HandleAsync_WhenServiceReturnsFailure_ReturnsTransientFailureWithUnhealthyYaml(
        HealthCheckOption option, string expectedFailureYaml)
    {
        var unhealthyMessage = "Dependency timeout";

        var healthyApiMock = BuildHealthCheckApiMock(
            serviceName: "Inventory",
            pingResult: default(Unit));

        var timeoutException = new InvalidOperationException("timeout");

        var unhealthyApiMock = BuildHealthCheckApiMock(
            serviceName: "Billing",
            pingResult: timeoutException.ToFailure(unhealthyMessage));

        var handler = new HealthCheckHandler(
            healthCheckApis:
            [
                healthyApiMock.Object,
                unhealthyApiMock.Object
            ],
            option: option);

        var actual = await handler.HandleAsync(default, TestContext.Current.CancellationToken);

        var expectedFailureMessage = NormalizeExpectedYaml(expectedFailureYaml);

        Assert.True(actual.IsFailure);
        var failure = actual.FailureOrThrow();

        Assert.Equal(HandlerFailureCode.Transient, failure.FailureCode);
        Assert.Same(timeoutException, failure.SourceException);
        Assert.Equal(expectedFailureMessage, failure.FailureMessage);
    }

    [Fact]
    public static async Task HandleAsync_WhenServiceThrowsException_ReturnsTransientFailureWithSourceException()
    {
        var sourceException = new InvalidOperationException("billing unavailable");

        var healthyApiMock = BuildHealthCheckApiMock(
            serviceName: "Inventory",
            pingResult: default(Unit));

        var unstableApiMock = BuildHealthCheckApiMock(
            serviceName: "Billing",
            pingException: sourceException);

        var handler = new HealthCheckHandler(
            healthCheckApis:
            [
                healthyApiMock.Object,
                unstableApiMock.Object
            ],
            option: default);

        var actual = await handler.HandleAsync(default, TestContext.Current.CancellationToken);

        var expectedFailureMessage = NormalizeExpectedYaml(
            """
            Info: 
            Status: Unhealthy
            Services:
              Inventory: Healthy
              Billing: "An unexpected exception occured: 'billing unavailable'"
            """);

        Assert.True(actual.IsFailure);
        var failure = actual.FailureOrThrow();

        Assert.Equal(HandlerFailureCode.Transient, failure.FailureCode);
        Assert.Same(sourceException, failure.SourceException);
        Assert.Equal(expectedFailureMessage, failure.FailureMessage);
    }

    [Fact]
    public static async Task HandleAsync_WhenMultipleServicesThrowExceptions_ReturnsAggregateException()
    {
        var firstException = new InvalidOperationException("first failure");
        var secondException = new ApplicationException("second failure");

        var firstApiMock = BuildHealthCheckApiMock(
            serviceName: "Inventory",
            pingException: firstException);

        var secondApiMock = BuildHealthCheckApiMock(
            serviceName: "Billing",
            pingException: secondException);

        var handler = new HealthCheckHandler(
            healthCheckApis:
            [
                firstApiMock.Object,
                secondApiMock.Object
            ],
            option: default);

        var actual = await handler.HandleAsync(default, TestContext.Current.CancellationToken);

        var expectedFailureMessage = NormalizeExpectedYaml(
            """
            Info: 
            Status: Unhealthy
            Services:
              Inventory: "An unexpected exception occured: 'first failure'"
              Billing: "An unexpected exception occured: 'second failure'"
            """);

        Assert.True(actual.IsFailure);

        var failure = actual.FailureOrThrow();

        Assert.Equal(HandlerFailureCode.Transient, failure.FailureCode);
        Assert.Equal(expectedFailureMessage, failure.FailureMessage);

        var aggregateException = Assert.IsType<AggregateException>(failure.SourceException);
        Assert.Contains(firstException, aggregateException.InnerExceptions);
        Assert.Contains(secondException, aggregateException.InnerExceptions);
    }

}
