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
    [Fact]
    public async void StartAsync_StopAsync_OnSuccess_Invokes_MainService_Main()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var applicationLifeTime = new ApplicationLifetime(new NullLogger<ApplicationLifetime>());
        var logger = new NullLogger<ApplicationHostedService>();
        var mainService = Mock.Of<IMainService>();
        var applicationHostedService = new ApplicationHostedService(applicationLifeTime, logger, mainService);

        // Act
        await applicationHostedService.StartAsync(cancellationToken).ConfigureAwait(false);
        applicationLifeTime.NotifyStarted();
        await applicationHostedService.StopAsync(cancellationToken).ConfigureAwait(false);

        // Assert
        Mock.Get(mainService)
            .Verify(service =>
                    service.Main(It.IsAny<string[]>(), It.IsAny<CancellationToken>()),
                Times.Once);
    }

    [Fact]
    public async void StartAsync_StopAsync_OnSuccess_ExitCode_Is_Success()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var applicationLifeTime = new ApplicationLifetime(new NullLogger<ApplicationLifetime>());
        var logger = new NullLogger<ApplicationHostedService>();
        var mainService = Mock.Of<IMainService>();
        var applicationHostedService = new ApplicationHostedService(applicationLifeTime, logger, mainService);

        Mock.Get(mainService)
            .Setup(service => service.Main(It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ExitCode.Success);

        // Act
        await applicationHostedService.StartAsync(cancellationToken).ConfigureAwait(false);
        applicationLifeTime.NotifyStarted();
        await applicationHostedService.StopAsync(cancellationToken).ConfigureAwait(false);

        // Assert
        Assert.Equal((int)ExitCode.Success, Environment.ExitCode);
    }

    [Fact]
    public async void StartAsync_StopAsync_OnEarlyCancel_Never_Invokes_MainService_Main()
    {
        // Arrange
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var applicationLifeTime = new ApplicationLifetime(new NullLogger<ApplicationLifetime>());
        var logger = new NullLogger<ApplicationHostedService>();
        var mainService = Mock.Of<IMainService>();
        var applicationHostedService = new ApplicationHostedService(applicationLifeTime, logger, mainService);

        // Act
        await cancellationTokenSource.CancelAsync().ConfigureAwait(false);
        await applicationHostedService.StartAsync(cancellationToken).ConfigureAwait(false);
        applicationLifeTime.NotifyStarted();
        await applicationHostedService.StopAsync(cancellationToken).ConfigureAwait(false);

        // Assert
        Mock.Get(mainService).Verify(service =>
                service.Main(It.IsAny<string[]>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async void StartAsync_StopAsync_OnEarlyCancel_ExitCode_Is_Aborted()
    {
        // Arrange
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var applicationLifeTime = new ApplicationLifetime(new NullLogger<ApplicationLifetime>());
        var logger = new NullLogger<ApplicationHostedService>();
        var mainService = Mock.Of<IMainService>();
        var applicationHostedService = new ApplicationHostedService(applicationLifeTime, logger, mainService);

        // Act
        await cancellationTokenSource.CancelAsync().ConfigureAwait(false);
        await applicationHostedService.StartAsync(cancellationToken).ConfigureAwait(false);
        applicationLifeTime.NotifyStarted();
        await applicationHostedService.StopAsync(cancellationToken).ConfigureAwait(false);

        // Assert
        Assert.Equal((int)ExitCode.Aborted, Environment.ExitCode);
    }

    [Fact]
    public async void StartAsync_StopAsync_OnTaskCancelledException_ExitCode_Is_Cancelled()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var applicationLifeTime = new ApplicationLifetime(new NullLogger<ApplicationLifetime>());
        var logger = new NullLogger<ApplicationHostedService>();
        var mainService = Mock.Of<IMainService>();
        var applicationHostedService = new ApplicationHostedService(applicationLifeTime, logger, mainService);

        Mock.Get(mainService)
            .Setup(service => service.Main(It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .Throws<TaskCanceledException>();

        // Act
        await applicationHostedService.StartAsync(cancellationToken).ConfigureAwait(false);
        applicationLifeTime.NotifyStarted();
        await applicationHostedService.StopAsync(cancellationToken).ConfigureAwait(false);

        // Assert
        Assert.Equal((int)ExitCode.Cancelled, Environment.ExitCode);
    }

    [Fact]
    public async void StartAsync_StopAsync_OnUnhandledException_ExitCode_Is_UnhandledException()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var applicationLifeTime = new ApplicationLifetime(new NullLogger<ApplicationLifetime>());
        var logger = new NullLogger<ApplicationHostedService>();
        var mainService = Mock.Of<IMainService>();
        var applicationHostedService = new ApplicationHostedService(applicationLifeTime, logger, mainService);

        Mock.Get(mainService)
            .Setup(service => service.Main(It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .Throws<Exception>();

        // Act
        await applicationHostedService.StartAsync(cancellationToken);
        applicationLifeTime.NotifyStarted();
        await applicationHostedService.StopAsync(cancellationToken);

        // Assert
        Assert.Equal((int)ExitCode.UnhandledException, Environment.ExitCode);
    }
}