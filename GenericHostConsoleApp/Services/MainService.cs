using System.Text.Json;
using GenericHostConsoleApp.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GenericHostConsoleApp.Services;

public sealed class MainService(
    IConfiguration configuration,
    ILogger<MainService> logger,
    IWeatherForecastService weatherForecastService) : IMainService
{
    private readonly IConfiguration _configuration =
        configuration ?? throw new ArgumentNullException(nameof(configuration));

    private readonly ILogger<MainService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    private readonly IWeatherForecastService _weatherForecastService =
        weatherForecastService ?? throw new ArgumentNullException(nameof(weatherForecastService));

    /// <summary>
    ///     Executes the main application logic.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>The exit code representing the outcome of the application execution.</returns>
    public async Task<ExitCode> Main(string[] args, CancellationToken cancellationToken)
    {
        try
        {
            var city = _configuration.GetValue<string>("City") ??
                       throw new InvalidOperationException("City not specified.");

            var forecastJson = await _weatherForecastService.FetchWeatherForecastAsync(city, cancellationToken)
                .ConfigureAwait(false);

            return ProcessWeatherForecast(forecastJson, city);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred during the main execution.");
            return ExitCode.UnhandledException;
        }
    }

    private ExitCode ProcessWeatherForecast(string forecastJson, string city)
    {
        try
        {
            using var root = JsonDocument.Parse(forecastJson);
            if (root.RootElement.ValueKind != JsonValueKind.Object)
            {
                _logger.LogError("Invalid JSON structure. Root element is not an object. JSON: {ForecastJson}",
                    forecastJson);
                return ExitCode.UnhandledException;
            }

            if (!root.RootElement.TryGetProperty("main", out var main) ||
                !main.TryGetProperty("temp", out var tempElement))
            {
                _logger.LogError("The JSON does not contain 'main' or 'temp' property. JSON: {ForecastJson}",
                    forecastJson);
                return ExitCode.UnhandledException;
            }

            var temp = KelvinToCelsius(tempElement.GetDouble());

            if (!root.RootElement.TryGetProperty("weather", out var weatherArray) ||
                weatherArray.ValueKind != JsonValueKind.Array || !weatherArray.EnumerateArray().Any())
            {
                _logger.LogError(
                    "The JSON does not contain 'weather' property or it is empty. JSON: {ForecastJson}",
                    forecastJson);
                return ExitCode.UnhandledException;
            }

            var weather = weatherArray.EnumerateArray().First();
            var weatherMain = weather.GetProperty("main").GetString();
            var weatherDescription = weather.GetProperty("description").GetString();

            if (string.IsNullOrEmpty(weatherMain) || string.IsNullOrEmpty(weatherDescription))
            {
                _logger.LogError(
                    "The 'weather' property does not contain valid 'main' or 'description' values. JSON: {ForecastJson}",
                    forecastJson);
                return ExitCode.UnhandledException;
            }

            _logger.LogInformation(
                "Weather Forecast for {City}: {Temperature:0}ºC - {WeatherMain} - {WeatherDescription}", city, temp,
                weatherMain, weatherDescription);
            return ExitCode.Success;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse weather forecast JSON.");
            return ExitCode.UnhandledException;
        }
    }

    /// <summary>
    ///     Converts a temperature from Kelvin to Celsius.
    /// </summary>
    /// <param name="kelvin">The temperature in Kelvin.</param>
    /// <returns>The temperature in Celsius.</returns>
    private static double KelvinToCelsius(double kelvin)
    {
        const double kelvinOffset = 273.15;
        return kelvin - kelvinOffset;
    }
}