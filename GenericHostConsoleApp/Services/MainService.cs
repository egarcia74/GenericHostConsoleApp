using System.Text.Json;
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
public sealed class MainService : IMainService
{
    // Service dependencies.
    private readonly IConfiguration _configuration;
    private readonly ILogger<MainService> _logger;
    private readonly IWeatherForecastService _weatherForecastService;

    /// <summary>
    ///     Initialises the <see cref="MainService" /> using the specified dependencies.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <param name="logger">The logger service.</param>
    /// <param name="weatherForecastService">The weather forecast service.</param>
    public MainService(
        IConfiguration configuration,
        ILogger<MainService> logger,
        IWeatherForecastService weatherForecastService)
    {
        _configuration = configuration;
        _logger = logger;
        _weatherForecastService = weatherForecastService;
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
        var city = _configuration.GetValue<string>("City");

        var forecastJson =
            await _weatherForecastService.FetchWeatherForecastAsync(city ?? throw new InvalidOperationException(),
                cancellationToken);

        using var root = JsonDocument.Parse(forecastJson);
        var main = root.RootElement.GetProperty("main");
        var temp = KelvinToCelsius(main.GetProperty("temp").GetDouble());
        var weather = root.RootElement.GetProperty("weather").EnumerateArray().First();
        var weatherMain = weather.GetProperty("main").GetString();
        var weatherDescription = weather.GetProperty("description").GetString();

        _logger.LogInformation("Weather Forecast for {city}: {temp:0} Cº - {weatherMain} - {weatherDescription}", city, temp, weatherMain, weatherDescription);

        return ExitCode.Success;
    }

    /// <summary>
    ///     Converts Kelvin to Celsius.
    /// </summary>
    /// <param name="kelvin">The temperature in Kelvin.</param>
    /// <returns>The Celcius temperature.</returns>
    private static double KelvinToCelsius(double kelvin)
    {
        return kelvin - 273.15;
    }
}