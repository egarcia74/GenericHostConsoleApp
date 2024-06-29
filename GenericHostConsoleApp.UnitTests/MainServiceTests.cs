using System;
using System.Threading;
using System.Threading.Tasks;
using GenericHostConsoleApp.Services;
using GenericHostConsoleApp.Services.Interfaces;
using Moq;
using Xunit;

namespace GenericHostConsoleApp.UnitTests;

public class MainServiceTests
{
    /// <summary>
    /// Initializes the test with necessary mocks and instances.
    /// </summary>
    /// <param name="weatherService">Mocked Weather service instance.</param>
    /// <param name="notificationService">Mocked User Notification service instance.</param>
    /// <param name="mainService">Main Service instance created with provided mocks.</param>
    /// <param name="temperatures">Array of integers representing temperatures.</param>
    private static void InitializeTest(
        out IWeatherService weatherService, 
        out IUserNotificationService notificationService,
        out MainService mainService, 
        out int[] temperatures)
    {
        weatherService = Mock.Of<IWeatherService>();
        notificationService = Mock.Of<IUserNotificationService>();
        mainService = new MainService(weatherService, notificationService);
        temperatures = [71, 72, 73, 74, 79 ];
    }

    [Fact]
    public async Task Main_OnSuccess_Returns_Success()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        
        InitializeTest(out var weatherService, out _, out var mainService, out var temperatures);

        Mock.Get(weatherService)
            .Setup(service => service.GetFiveDayTemperaturesAsync(It.IsAny<CancellationToken>()))
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
        
        InitializeTest(out var weatherService, out var notificationService, out var mainService, out var temperatures);

        Mock.Get(weatherService)
            .Setup(service => service.GetFiveDayTemperaturesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(temperatures);

        // Act
        _ = await mainService.Main([], cancellationToken);

        // Assert
        for (var i = 0; i < temperatures.Length; i++)
        {
            var dayOfWeek = DateTime.Today.AddDays(i).DayOfWeek;
            var dayTemperature = temperatures[i];

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
        
        InitializeTest(out var weatherService, out _, out var mainService, out var temperatures);

        await cancellationTokenSource.CancelAsync();

        Mock.Get(weatherService)
            .Setup(service => service.GetFiveDayTemperaturesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(temperatures);

        // Act / assert
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            _ = await mainService.Main([], cancellationToken));
    }
}