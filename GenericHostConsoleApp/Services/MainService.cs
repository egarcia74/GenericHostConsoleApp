using GenericHostConsoleApp.Models.WeatherForecast;
using GenericHostConsoleApp.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GenericHostConsoleApp.Services;

/// <summary>
///     Main service responsible for executing the core application logic.
/// </summary>
public sealed class MainService(
    IConfiguration configuration,
    ILogger<MainService> logger,
    IWeatherForecastService weatherForecastService) : IMainService
{
    /// <summary>
    ///     Executes the main functionality of the application.
    /// </summary>
    /// <param name="args">An array of command-line arguments passed to the application.</param>
    /// <param name="cancellationToken">A token that can be used to signal the operation should be canceled.</param>
    /// <returns>Returns an <see cref="ExitCode" /> indicating the result of the execution.</returns>
    public async Task<ExitCode> ExecuteMainAsync(string[] args, CancellationToken cancellationToken)
    {
        var city = GetCityFromConfiguration();

        var weatherForecast = await weatherForecastService
            .FetchWeatherForecastAsync(city, cancellationToken)
            .ConfigureAwait(false);

        ProcessWeatherForecast(weatherForecast);

        return ExitCode.Success;
    }

    /// <summary>
    ///     Retrieves the city name from the application's configuration.
    /// </summary>
    /// <returns>
    ///     Returns the city name as a string if it exists in the configuration; otherwise, throws an
    ///     InvalidOperationException.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the city configuration key is not specified or the value is
    ///     empty.
    /// </exception>
    private string GetCityFromConfiguration()
    {
        const string cityConfigKey = "City";
        var city = configuration.GetValue<string>(cityConfigKey);
        if (string.IsNullOrEmpty(city))
            throw new InvalidOperationException($"Configuration key '{cityConfigKey}' not specified.");
        return city;
    }

    /// <summary>
    ///     Processes the weather forecast data by converting temperature values
    ///     from Kelvin to Celsius and logging the information.
    /// </summary>
    /// <param name="weatherResponse">The weather response data containing the forecast details.</param>
    private void ProcessWeatherForecast(WeatherResponse weatherResponse)
    {
        ArgumentNullException.ThrowIfNull(weatherResponse);

        if (weatherResponse.Main == null || weatherResponse.Weather == null || weatherResponse.Weather.Count == 0)
        {
            logger.LogWarning("Weather response does not contain necessary data");
            return;
        }

        var temperatureUnit = configuration.GetValue<TemperatureUnit>("TemperatureUnit");
        var temperatureUnitCode = temperatureUnit == TemperatureUnit.Celsius ? "C" : "F";
        var temperature = TemperatureConverter.ConvertTemperature(weatherResponse.Main.Temp, TemperatureUnit.Kelvin,
            temperatureUnit == TemperatureUnit.Celsius ? TemperatureUnit.Celsius : TemperatureUnit.Fahrenheit);
        var feelsLike = TemperatureConverter.ConvertTemperature(weatherResponse.Main.FeelsLike, TemperatureUnit.Kelvin,
            temperatureUnit == TemperatureUnit.Celsius ? TemperatureUnit.Celsius : TemperatureUnit.Fahrenheit);
        var tempMin = TemperatureConverter.ConvertTemperature(weatherResponse.Main.TempMin, TemperatureUnit.Kelvin,
            temperatureUnit == TemperatureUnit.Celsius ? TemperatureUnit.Celsius : TemperatureUnit.Fahrenheit);
        var tempMax = TemperatureConverter.ConvertTemperature(weatherResponse.Main.TempMax, TemperatureUnit.Kelvin,
            temperatureUnit == TemperatureUnit.Celsius ? TemperatureUnit.Celsius : TemperatureUnit.Fahrenheit);

        var logMessage =
            $"Weather Forecast for {weatherResponse.Name} is {temperature:0}º{temperatureUnitCode} " +
            $"(feels like {feelsLike:0}º{temperatureUnitCode}) " + 
            $"Min {tempMin:0}º{temperatureUnitCode} " +
            $"Max {tempMax:0}º{temperatureUnitCode} - " + 
            $"{weatherResponse.Weather.First().Description}";

        logger.LogInformation(
            logMessage,
            weatherResponse.Name,
            temperature,
            feelsLike,
            tempMin,
            tempMax,
            weatherResponse.Weather.First().Description);
    }


    /// <summary>
    ///     Converts a temperature value from Kelvin to Celsius.
    /// </summary>
    /// <param name="kelvin">The temperature value in Kelvin.</param>
    /// <returns>The temperature value converted to Celsius.</returns>
    private static double KelvinToCelsius(double kelvin)
    {
        const double kelvinOffset = 273.15;
        return kelvin - kelvinOffset;
    }

    public static double KelvinToFahrenheit(double kelvin)
    {
        const double kelvinOffset = 273.15;
        return (kelvin - kelvinOffset) * 9 / 5 + 32;
    }
}