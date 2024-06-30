using System;
using System.Threading;
using System.Threading.Tasks;
using GenericHostConsoleApp.Services;
using GenericHostConsoleApp.Services.Interfaces;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace GenericHostConsoleApp.UnitTests;

public class ApplicationHostedServiceTests
{
    /// <summary>
    ///     Initializes the test with necessary instances and mocks.
    /// </summary>
    /// <param name="applicationLifeTime">Application lifetime.</param>
    /// <param name="logger">Logger instance for ApplicationHostedService.</param>
    /// <param name="mainService">Mocked Main Service instance.</param>
    /// <param name="applicationHostedService">Application Hosted Service using the provided instances.</param>
    private static void InitializeTest(
        out ApplicationLifetime applicationLifeTime,
        out NullLogger<ApplicationHostedService> logger,
        out IMainService mainService,
        out ApplicationHostedService applicationHostedService)
    {
        applicationLifeTime = new ApplicationLifetime(new NullLogger<ApplicationLifetime>());
        logger = new NullLogger<ApplicationHostedService>();
        mainService = Mock.Of<IMainService>();
        applicationHostedService = new ApplicationHostedService(applicationLifeTime, logger, mainService);
    }

    [Fact]
    public async Task StartAsync_StopAsync_OnSuccess_Invokes_MainService_Main()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        InitializeTest(out var applicationLifeTime, out _, out var mainService, out var applicationHostedService);

        // Act
        await applicationHostedService.StartAsync(cancellationToken);
        applicationLifeTime.NotifyStarted();
        await applicationHostedService.StopAsync(cancellationToken);

        // Assert
        Mock.Get(mainService)
            .Verify(service =>
                    service.Main(It.IsAny<string[]>(), It.IsAny<CancellationToken>()),
                Times.Once);
    }

    [Fact]
    public async Task StartAsync_StopAsync_OnSuccess_ExitCode_Is_Success()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        InitializeTest(out var applicationLifeTime, out _, out var mainService, out var applicationHostedService);

        Mock.Get(mainService)
            .Setup(service => service.Main(It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ExitCode.Success);

        // Act
        await applicationHostedService.StartAsync(cancellationToken);
        applicationLifeTime.NotifyStarted();
        await applicationHostedService.StopAsync(cancellationToken);

        // Assert
        Assert.Equal((int)ExitCode.Success, Environment.ExitCode);
    }

    [Fact]
    public async Task StartAsync_StopAsync_OnEarlyCancel_Never_Invokes_MainService_Main()
    {
        // Arrange
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        InitializeTest(out var applicationLifeTime, out _, out var mainService, out var applicationHostedService);

        // Act
        await cancellationTokenSource.CancelAsync();
        await applicationHostedService.StartAsync(cancellationToken);
        applicationLifeTime.NotifyStarted();
        await applicationHostedService.StopAsync(cancellationToken);

        // Assert
        Mock.Get(mainService).Verify(service =>
                service.Main(It.IsAny<string[]>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task StartAsync_StopAsync_OnEarlyCancel_ExitCode_Is_Aborted()
    {
        // Arrange
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        InitializeTest(out var applicationLifeTime, out _, out _, out var applicationHostedService);

        // Act
        await cancellationTokenSource.CancelAsync();
        await applicationHostedService.StartAsync(cancellationToken);
        applicationLifeTime.NotifyStarted();
        await applicationHostedService.StopAsync(cancellationToken);

        // Assert
        Assert.Equal((int)ExitCode.Aborted, Environment.ExitCode);
    }

    [Fact]
    public async Task StartAsync_StopAsync_OnTaskCancelledException_ExitCode_Is_Cancelled()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        InitializeTest(out var applicationLifeTime, out _, out var mainService, out var applicationHostedService);

        Mock.Get(mainService)
            .Setup(service => service.Main(It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .Throws<TaskCanceledException>();

        // Act
        await applicationHostedService.StartAsync(cancellationToken);
        applicationLifeTime.NotifyStarted();
        await applicationHostedService.StopAsync(cancellationToken);

        // Assert
        Assert.Equal((int)ExitCode.Cancelled, Environment.ExitCode);
    }

    [Fact]
    public async Task StartAsync_StopAsync_OnArgumentNullException_ExitCode_Is_ArgumentNullException()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        InitializeTest(out var applicationLifeTime, out _, out var mainService, out var applicationHostedService);

        Mock.Get(mainService)
            .Setup(service => service.Main(It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .Throws<ArgumentNullException>();

        // Act
        await applicationHostedService.StartAsync(cancellationToken);
        applicationLifeTime.NotifyStarted();
        await applicationHostedService.StopAsync(cancellationToken);

        // Assert
        Assert.Equal((int)ExitCode.ArgumentNullException, Environment.ExitCode);
    }

    [Fact]
    public async Task StartAsync_StopAsync_OnInvalidOperationException_ExitCode_Is_InvalidOperationException()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        InitializeTest(out var applicationLifeTime, out _, out var mainService, out var applicationHostedService);

        Mock.Get(mainService)
            .Setup(service => service.Main(It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .Throws<InvalidOperationException>();

        // Act
        await applicationHostedService.StartAsync(cancellationToken);
        applicationLifeTime.NotifyStarted();
        await applicationHostedService.StopAsync(cancellationToken);

        // Assert
        Assert.Equal((int)ExitCode.InvalidOperationException, Environment.ExitCode);
    }
}