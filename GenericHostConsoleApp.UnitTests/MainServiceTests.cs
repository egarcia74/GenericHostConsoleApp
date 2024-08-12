using System;
using System.Threading;
using System.Threading.Tasks;
using GenericHostConsoleApp.Services;
using GenericHostConsoleApp.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GenericHostConsoleApp.UnitTests;

public class MainServiceTests
{
    /// <summary>
    ///     Initializes the test with necessary mocks and instances.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="weatherForecastService">Mocked Weather service instance.</param>
    /// <param name="notificationService">Mocked User Notification service instance.</param>
    /// <param name="mainService">Main Service instance created with provided mocks.</param>
    /// <param name="forecast">The weather forecast.</param>
    private static void InitializeTest(
        // ReSharper disable once OutParameterValueIsAlwaysDiscarded.Local
        out ILogger<MainService> logger,
        out IWeatherForecastService weatherForecastService,
        out IUserNotificationService notificationService,
        out MainService mainService,
        out string forecast)
    {
        logger = Mock.Of<ILogger<MainService>>();
        weatherForecastService = Mock.Of<IWeatherForecastService>();
        notificationService = Mock.Of<IUserNotificationService>();
        mainService = new MainService(logger, weatherForecastService);
        forecast = "forecast";
    }

    [Fact]
    public async Task Main_OnSuccess_Returns_Success()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        InitializeTest(out _, out var weatherForecastService, out _, out var mainService, out var temperatures);

        Mock.Get(weatherForecastService)
            .Setup(service => service.FetchWeatherForecastAsync(cancellationToken))
            .ReturnsAsync(temperatures);

        // Act
        var exitCode = await mainService.Main([], cancellationToken);

        // Assert
        Assert.Equal(ExitCode.Success, exitCode);
    }

    [Fact]
    public async Task Main_OnSuccess_Invokes_UserNotificationService_NotifyDailyWeatherAsync()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        InitializeTest(out _, out var weatherForecastService, out var notificationService, out var mainService,
            out var forecast);

        Mock.Get(weatherForecastService)
            .Setup(service => service.FetchWeatherForecastAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(forecast);

        // Act
        _ = await mainService.Main([], cancellationToken);

        // Assert
        for (var i = 0; i < forecast.Length; i++)
        {
            var dayOfWeek = DateTime.Today.AddDays(i).DayOfWeek;
            var dayTemperature = forecast[i];

            Mock.Get(notificationService)
                .Verify(notificationServiceInner =>
                        notificationServiceInner.NotifyDailyWeatherAsync(dayOfWeek, dayTemperature),
                    Times.Once);
        }
    }

    [Fact]
    public async Task Main_OnCancelled_Throws_OperationCanceledException()
    {
        // Arrange
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        InitializeTest(out _, out var weatherService, out _, out var mainService, out var temperatures);

        await cancellationTokenSource.CancelAsync();

        Mock.Get(weatherService)
            .Setup(service => service.FetchWeatherForecastAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(temperatures);

        // Act / assert
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            _ = await mainService.Main([], cancellationToken));
    }
}