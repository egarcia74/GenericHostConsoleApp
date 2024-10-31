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
    /// Retrieves the temperature unit configuration value from the application settings.
    /// </summary>
    /// <returns>Returns the configured temperature unit of type <see cref="TemperatureUnit" />.</returns>
    private TemperatureUnit GetTemperatureUnitFromConfiguration()
    {
        const string temperatureUnitKey = "TemperatureUnit";

        if (string.IsNullOrEmpty(configuration.GetValue<String>(temperatureUnitKey)))
        {
            throw new InvalidOperationException($"Configuration key '{temperatureUnitKey}' not specified.");
        }

        var temperatureUnit = configuration.GetValue<TemperatureUnit>(temperatureUnitKey);

        return temperatureUnit;
    }

    /// <summary>
    ///     Processes the weather forecast data by converting temperature values
    ///     from Kelvin to Celsius and logging the information.
    /// </summary>
    /// <param name="weatherResponse">The weather response data containing the forecast details.</param>
    private void ProcessWeatherForecast(WeatherResponse weatherResponse)
    {
        ArgumentNullException.ThrowIfNull(weatherResponse);

        if (weatherResponse.Main is null || weatherResponse.Weather is null || weatherResponse.Weather.Count == 0)
        {
            logger.LogWarning("Weather response does not contain necessary data");
            return;
        }
        
        var temperatureUnit = GetTemperatureUnitFromConfiguration();

        LogWeather(weatherResponse, temperatureUnit);
    }

    /// <summary>
    ///     Logs the weather forecast details in the specified temperature unit.
    /// </summary>
    /// <param name="weatherResponse">The weather response data to be logged.</param>
    /// <param name="temperatureUnit">The unit in which the temperature should be logged (Celsius, Fahrenheit, or Kelvin).</param>
    private void LogWeather(WeatherResponse weatherResponse, TemperatureUnit temperatureUnit)
    {
        var temperature = TemperatureConverter.ConvertTemperature(
            weatherResponse.Main!.Temp,
            TemperatureUnit.Kelvin,
            temperatureUnit);

        var feelsLike = TemperatureConverter.ConvertTemperature(
            weatherResponse.Main.FeelsLike,
            TemperatureUnit.Kelvin,
            temperatureUnit);

        var tempMin = TemperatureConverter.ConvertTemperature(
            weatherResponse.Main.TempMin,
            TemperatureUnit.Kelvin,
            temperatureUnit);

        var tempMax = TemperatureConverter.ConvertTemperature(
            weatherResponse.Main.TempMax,
            TemperatureUnit.Kelvin,
            temperatureUnit);

        switch (temperatureUnit)
        {
            case TemperatureUnit.Celsius:
                logger.LogInformation(
                    "Weather forecast for {City} is {Temperature:0}ºC (feels like {FeelsLike:0}ºC) Min {TempMin:0}ºC Max {TempMax:0}ºC {WeatherDescription}",
                    weatherResponse.Name,
                    temperature,
                    feelsLike,
                    tempMin,
                    tempMax,
                    weatherResponse.Weather!.First().Description);
                break;

            case TemperatureUnit.Fahrenheit:
                logger.LogInformation(
                    "Weather forecast for {City} is {Temperature:0}ºF (feels like {FeelsLike:0}ºF) Min {TempMin:0}ºF Max {TempMax:0}ºF {WeatherDescription}",
                    weatherResponse.Name,
                    temperature,
                    feelsLike,
                    tempMin,
                    tempMax,
                    weatherResponse.Weather!.First().Description);
                break;

            case TemperatureUnit.Kelvin:
                logger.LogInformation(
                    "Weather forecast for {City} is {Temperature:0}ºK (feels like {FeelsLike:0}ºK) Min {TempMin:0}ºK Max {TempMax:0}ºK {WeatherDescription}",
                    weatherResponse.Name,
                    temperature,
                    feelsLike,
                    tempMin,
                    tempMax,
                    weatherResponse.Weather!.First().Description);
                break;

            default:
                throw new InvalidOperationException($"Unknown temperature unit: {temperatureUnit}");
        }
    }
}