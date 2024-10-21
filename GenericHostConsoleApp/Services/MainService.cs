using System.Text.Json;
using GenericHostConsoleApp.Models.WeatherForecast;
using GenericHostConsoleApp.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GenericHostConsoleApp.Services;

/// <summary>
/// MainService is a core class responsible for orchestrating the main
/// application logic, primarily interacting with various services and
/// handling the application's entry-point activities.
/// </summary>
public sealed class MainService : IMainService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<MainService> _logger;
    private readonly IWeatherForecastService _weatherForecastService;

    public MainService(IConfiguration configuration,
        ILogger<MainService> logger,
        IWeatherForecastService weatherForecastService)
    {
        _configuration = configuration;
        _logger = logger;
        _weatherForecastService = weatherForecastService;
    }

    /// <summary>
    /// The key used to retrieve the city configuration value from the application's configuration settings.
    /// </summary>
    private const string CityConfigKey = "City";

    /// <summary>
    /// Executes the main functionality of the application.
    /// </summary>
    /// <param name="args">An array of command-line arguments passed to the application.</param>
    /// <param name="cancellationToken">A token that can be used to signal the operation should be canceled.</param>
    /// <returns>Returns an <see cref="ExitCode"/> indicating the result of the execution.</returns>
    public async Task<ExitCode> MainAsync(string[] args, CancellationToken cancellationToken) // Renamed to MainAsync
    {
        try
        {
            var city = GetCityFromConfiguration();
            
            var forecastJson = await _weatherForecastService
                .FetchWeatherForecastAsync(city, cancellationToken)
                .ConfigureAwait(false);

            return ProcessWeatherForecast(forecastJson);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "An argument exception occurred during the main execution");
            return ExitCode.InvalidArgument;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "An invalid operation exception occurred during the main execution");
            return ExitCode.InvalidOperation;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "A JSON exception occurred during the main execution");
            return ExitCode.InvalidJson;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected exception occurred during the main execution");
            return ExitCode.UnhandledException;
        }
    }

    /// <summary>
    /// Retrieves the city name from the application's configuration.
    /// </summary>
    /// <returns>Returns the city name as a string if it exists in the configuration; otherwise, throws an InvalidOperationException.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the city configuration key is not specified or the value is empty.</exception>
    private string GetCityFromConfiguration()
    {
        var city = _configuration.GetValue<string>(CityConfigKey);
        if (string.IsNullOrEmpty(city))
        {
            throw new InvalidOperationException($"Configuration key '{CityConfigKey}' not specified.");
        }

        return city;
    }

    /// <summary>
    /// Processes the weather forecast data for a specified city.
    /// </summary>
    /// <param name="forecastJson">The weather forecast data in JSON format.</param>
    /// <param name="city">The name of the city for which the weather forecast is being processed.</param>
    /// <returns>Returns an <see cref="ExitCode"/> indicating the result of the processing.</returns>
    private ExitCode ProcessWeatherForecast(string forecastJson)
    {
        try
        {
            var weatherResponse = JsonSerializer.Deserialize<WeatherResponse>(forecastJson);
            if (weatherResponse == null)
            {
                throw new JsonException("Failed to deserialize weather forecast.");
            }

            var temperature = KelvinToCelsius(weatherResponse.Main!.Temp);

            _logger.LogInformation(
                "Weather Forecast for {City}: {Temperature:0}ºC - {WeatherMain} - {WeatherDescription}",
                weatherResponse.Name,
                temperature,
                weatherResponse.Weather?.First().Main,
                weatherResponse.Weather?.First().Description);

            return ExitCode.Success;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse weather forecast JSON");
            return ExitCode.InvalidJson;
        }
    }

    /// <summary>
    /// Converts a temperature value from Kelvin to Celsius.
    /// </summary>
    /// <param name="kelvin">The temperature value in Kelvin.</param>
    /// <returns>The temperature value converted to Celsius.</returns>
    private static double KelvinToCelsius(double kelvin)
    {
        const double kelvinOffset = 273.15;
        return kelvin - kelvinOffset;
    }
}