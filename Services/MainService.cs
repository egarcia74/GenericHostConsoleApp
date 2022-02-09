using GenericHostConsoleApp.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GenericHostConsoleApp.Services;

/// <summary>
///     Main application service.
/// </summary>
/// <remarks>
///     This service implements the main application logic.
///     The method <see cref="Main" /> is executed once when the application executes.
/// </remarks>
public sealed partial class MainService : IMainService
{
    // Service dependencies.
    private readonly IConfiguration _configuration;
    private readonly ILogger<MainService> _logger;
    private readonly IWeatherService _weatherService;

    /// <summary>
    ///     Initialises the <see cref="MainService" /> using the specified dependencies.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="logger">The logger to use within this service.</param>
    /// <param name="weatherService">The weather service.</param>
    public MainService(IConfiguration configuration, ILogger<MainService> logger, IWeatherService weatherService)
    {
        _configuration = configuration;
        _logger = logger;
        _weatherService = weatherService;
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
        // Command line arguments can be accessed either via the args parameter or via configuration.
        LogConfigurationDetails("arg1", _configuration["arg1"]);
        LogConfigurationDetails("arg2", _configuration["arg2"]);

        var temperatures = await _weatherService.GetFiveDayTemperaturesAsync(cancellationToken)
            .ConfigureAwait(false);

        for (var i = 0; i < temperatures.Count; i++)
        {
            if (cancellationToken.IsCancellationRequested) break;

            LogDailyForecast(DateTime.Today.AddDays(i).DayOfWeek, temperatures[i]);
        }

        return ExitCode.Success;
    }

    /// <summary>
    ///     Logs details of configuration data.
    /// </summary>
    /// <param name="name">The name of the configuration item.</param>
    /// <param name="value">The value of the configuration item.</param>
    [LoggerMessage(100, LogLevel.Information, "Configuration: {Name} = {Value}")]
    partial void LogConfigurationDetails(string name, string value);

    /// <summary>
    ///     Logs a message indicating that the application was cancelled.
    /// </summary>
    /// <param name="dayOfWeek">The day of the week.</param>
    /// <param name="temperature">The temperature for the day.</param>
    [LoggerMessage(101, LogLevel.Information, "The forecast for {DayOfWeek} is {Temperature}.")]
    partial void LogDailyForecast(DayOfWeek dayOfWeek, int temperature);
}