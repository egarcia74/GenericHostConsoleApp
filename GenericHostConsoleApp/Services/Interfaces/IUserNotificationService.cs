namespace GenericHostConsoleApp.Services.Interfaces;

/// <summary>
///     User notification service interface.
/// </summary>
public interface IUserNotificationService
{
    /// <summary>
    ///     Notifies the user of the daily weather forecast.
    /// </summary>
    /// <param name="dayOfWeek">The day of the week.</param>
    /// <param name="dayTemperature">The day's temperature.</param>
    /// <returns>A <see cref="Task" /> that can be awaited.</returns>
    Task NotifyDailyWeatherAsync(DayOfWeek dayOfWeek, int dayTemperature);
}