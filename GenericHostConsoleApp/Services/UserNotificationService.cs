using GenericHostConsoleApp.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GenericHostConsoleApp.Services;

/// <summary>
///     Handles user notifications from the application.
/// </summary>
public partial class UserNotificationService : IUserNotificationService
{
    // Service dependencies
    private readonly ILogger<UserNotificationService> _logger;

    public UserNotificationService(ILogger<UserNotificationService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    ///     Notifies the user of the daily weather forecast.
    /// </summary>
    /// <param name="dayOfWeek">The day of the week.</param>
    /// <param name="dayTemperature">The day's temperature.</param>
    /// <returns>A <see cref="Task" /> that can be awaited.</returns>
    public Task NotifyDailyWeatherAsync(DayOfWeek dayOfWeek, int dayTemperature)
    {
        LogDailyForecast(dayOfWeek, dayTemperature);

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Logs a message indicating that the application was cancelled.
    /// </summary>
    /// <param name="dayOfWeek">The day of the week.</param>
    /// <param name="temperature">The temperature for the day.</param>
    [LoggerMessage(100, LogLevel.Information, "The forecast for {DayOfWeek} is {Temperature}")]
    partial void LogDailyForecast(DayOfWeek dayOfWeek, int temperature);
}