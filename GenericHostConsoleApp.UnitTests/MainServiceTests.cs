using System;
using System.Threading;
using GenericHostConsoleApp.Services;
using GenericHostConsoleApp.Services.Interfaces;
using Moq;
using Xunit;

namespace GenericHostConsoleApp.UnitTests;

public class MainServiceTests
{
    [Fact]
    public async void Main_OnSuccess_Returns_Success()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var weatherService = Mock.Of<IWeatherService>();
        var notificationService = Mock.Of<IUserNotificationService>();
        var mainService = new MainService(weatherService, notificationService);
        var temperatures = new[] { 71, 72, 73, 74, 79 };

        Mock.Get(weatherService)
            .Setup(service => service.GetFiveDayTemperaturesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(temperatures);

        // Act
        var exitCode = await mainService.Main(Array.Empty<string>(), cancellationToken);

        // Assert
        Assert.Equal(ExitCode.Success, exitCode);
    }

    [Fact]
    public async void Main_OnSuccess_Invokes_UserNotificationService_NotifyDailyWeatherAsync()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var weatherService = Mock.Of<IWeatherService>();
        var notificationService = Mock.Of<IUserNotificationService>();
        var mainService = new MainService(weatherService, notificationService);
        var temperatures = new[] { 71, 72, 73, 74, 79 };

        Mock.Get(weatherService)
            .Setup(service => service.GetFiveDayTemperaturesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(temperatures);

        // Act
        _ = await mainService.Main(Array.Empty<string>(), cancellationToken);

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
    public async void Main_OnCancelled_Throws_OperationCanceledException()
    {
        // Arrange
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var weatherService = Mock.Of<IWeatherService>();
        var notificationService = Mock.Of<IUserNotificationService>();
        var mainService = new MainService(weatherService, notificationService);
        var temperatures = new[] { 71, 72, 73, 74, 79 };

        cancellationTokenSource.Cancel();

        Mock.Get(weatherService)
            .Setup(service => service.GetFiveDayTemperaturesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(temperatures);

        // Act / assert
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            _ = await mainService.Main(Array.Empty<string>(), cancellationToken));
    }
}