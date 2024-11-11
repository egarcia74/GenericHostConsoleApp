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
    ///     Retrieves the name of the place to get the weather for from the application's configuration.
    /// </summary>
    /// <value>
    ///     Returns the name as a string if it exists in the configuration; otherwise, throws an
    ///     InvalidOperationException.
    /// </value>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the name configuration key is not specified or the value is
    ///     empty.
    /// </exception>
    private string Name
    {
        get
        {
            const string nameKey = "Name";

            var name = configuration.GetValue<string>(nameKey);

            if (string.IsNullOrEmpty(name))
                throw new InvalidOperationException(
                    $"Configuration key '{nameKey}' not specified. Please specify a place name using --name \"Place Name\".");

            return name;
        }
    }

    /// <summary>
    ///     Retrieves the temperature unit configuration value from the application settings.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    /// <value>Returns the configured temperature unit of type <see cref="TemperatureUnit" />.</value>
    private TemperatureUnit TemperatureUnit
    {
        get
        {
            const string temperatureUnitKey = "TemperatureUnit";

            if (string.IsNullOrEmpty(configuration.GetValue<string>(temperatureUnitKey)))
                throw new InvalidOperationException($"Configuration key '{temperatureUnitKey}' not specified.");

            var temperatureUnit = configuration.GetValue<TemperatureUnit>(temperatureUnitKey);

            return temperatureUnit;
        }
    }

    /// <summary>
    ///     Executes the main functionality of the application.
    /// </summary>
    /// <param name="args">An array of command-line arguments passed to the application.</param>
    /// <param name="cancellationToken">A token that can be used to signal the operation should be canceled.</param>
    /// <returns>Returns an <see cref="ExitCode" /> indicating the result of the execution.</returns>
    public async Task<ExitCode> ExecuteMainAsync(string[] args, CancellationToken cancellationToken)
    {
#pragma warning disable CS0168

        var weatherForecast = await weatherForecastService
            .FetchWeatherForecastAsync(Name, cancellationToken)
            .ConfigureAwait(false);

        ProcessWeatherForecast(weatherForecast);

        return ExitCode.Success;

#pragma warning restore CS0168
    }

    /// <summary>
    ///     Processes the weather forecast data by converting temperature values
    ///     from Kelvin to Celsius and logging the information.
    /// </summary>
    /// <param name="weatherResponse">The weather response data containing the forecast details.</param>
    /// <exception cref="InvalidOperationException">Weather response does not contain necessary data.</exception>
    private void ProcessWeatherForecast(WeatherResponse weatherResponse)
    {
        ArgumentNullException.ThrowIfNull(weatherResponse);

        if (weatherResponse.Main is null || weatherResponse.Weather is null || weatherResponse.Weather.Count == 0 ||
            weatherResponse.Sys is null)
            throw new InvalidOperationException("Weather response does not contain necessary data.");

        LogWeather(weatherResponse, TemperatureUnit);
    }

    /// <summary>
    ///     Logs the weather forecast details in the specified temperature unit.
    /// </summary>
    /// <param name="weatherResponse">The weather response data to be logged.</param>
    /// <param name="temperatureUnit">The unit in which the temperature should be logged (Celsius, Fahrenheit, or Kelvin).</param>
    /// <exception cref="InvalidOperationException">Unknown temperature unit.</exception>
    private void LogWeather(WeatherResponse weatherResponse, TemperatureUnit temperatureUnit)
    {
        var temperature = TemperatureConverter.ConvertTemperature(weatherResponse.Main!.Temp, TemperatureUnit.Kelvin, temperatureUnit);
        var feelsLike = TemperatureConverter.ConvertTemperature(weatherResponse.Main.FeelsLike, TemperatureUnit.Kelvin, temperatureUnit);
        var tempMin = TemperatureConverter.ConvertTemperature(weatherResponse.Main.TempMin, TemperatureUnit.Kelvin, temperatureUnit);
        var tempMax = TemperatureConverter.ConvertTemperature(weatherResponse.Main.TempMax, TemperatureUnit.Kelvin, temperatureUnit);

        string unitSymbol = temperatureUnit switch
        {
            TemperatureUnit.Celsius => "ºC",
            TemperatureUnit.Fahrenheit => "ºF",
            TemperatureUnit.Kelvin => "ºK",
            _ => throw new InvalidOperationException($"Unknown temperature unit: {temperatureUnit}")
        };

        logger.LogInformation(
            "Weather forecast for {Name}, {Country}: Temperature: {Temperature:0}{UnitSymbol} (feels like {FeelsLike:0}{UnitSymbol}), Min: {TempMin:0}{UnitSymbol}, Max: {TempMax:0}{UnitSymbol}. Weather: {WeatherDescription}",
            weatherResponse.Name,
            weatherResponse.Sys?.Country,
            temperature,
            unitSymbol,
            feelsLike,
            unitSymbol,
            tempMin,
            unitSymbol,
            tempMax,
            unitSymbol,
            weatherResponse.Weather!.First().Description);
    }}