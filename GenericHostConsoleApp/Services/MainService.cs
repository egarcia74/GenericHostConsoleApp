using GenericHostConsoleApp.Services.Interfaces;

namespace GenericHostConsoleApp.Services;

/// <summary>
///     Main application service.
/// </summary>
/// <remarks>
///     This service implements the main application logic.
///     The method <see cref="Main" /> is executed once when the application executes.
/// </remarks>
public sealed class MainService : IMainService
{
    private readonly IUserNotificationService _userNotificationService;

    // Service dependencies.
    private readonly IWeatherService _weatherService;

    /// <summary>
    ///     Initialises the <see cref="MainService" /> using the specified dependencies.
    /// </summary>
    /// <param name="weatherService">The weather service.</param>
    /// <param name="userNotificationService">The notification service.</param>
    public MainService(IWeatherService weatherService, IUserNotificationService userNotificationService)
    {
        _weatherService = weatherService;
        _userNotificationService = userNotificationService;
    }

    /// <summary>
    ///     Executes the main application logic.
    /// </summary>
    /// <remarks>
    ///     This method is executed once when the application executes.
    /// </remarks>
    /// <param name="args">The command line arguments.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The application exit code.</returns>
    public async Task<ExitCode> Main(string[] args, CancellationToken cancellationToken)
    {
        var temperatures = await _weatherService.GetFiveDayTemperaturesAsync(cancellationToken)
            .ConfigureAwait(false);

        for (var i = 0; i < temperatures.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _userNotificationService.NotifyDailyWeatherAsync(DateTime.Today.AddDays(i).DayOfWeek,
                temperatures[i]);
        }

        return ExitCode.Success;
    }
}