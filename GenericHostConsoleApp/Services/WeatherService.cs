using GenericHostConsoleApp.Configuration;
using GenericHostConsoleApp.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GenericHostConsoleApp.Services;

/// <summary>
///     Fictional weather service.
/// </summary>
public sealed partial class WeatherService : IWeatherService
{
    // Service dependencies.
    private readonly ILogger<WeatherService> _logger;
    private readonly IOptions<WeatherOptions> _options;

    /// <summary>
    ///     Initialises the <see cref="WeatherService" /> using the specified dependencies.
    /// </summary>
    /// <param name="options">Configuration options.</param>
    /// <param name="logger">The logger to use within this service.</param>
    public WeatherService(
        IOptions<WeatherOptions> options,
        ILogger<WeatherService> logger
    )
    {
        _logger = logger;
        _options = options;
    }

    /// <summary>
    ///     Fictional weather service method.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A 5-day weather forecast.</returns>
    public async Task<IReadOnlyList<int>> GetFiveDayTemperaturesAsync(CancellationToken cancellationToken)
    {
        // Using Serilog's destructuring operator (@) in a log message.
        _logger.LogInformation("Using options {@Options}", _options.Value);

        LogFetchingWeather();

        // Simulate some network latency
        await Task.Delay(2000, cancellationToken).ConfigureAwait(false);

        int[] temperatures = [76, 76, 77, 79, 78];

        temperatures = ConvertToFahrenheit(temperatures);

        LogFetchWeatherSuccess();

        return temperatures;
    }

    /// <summary>
    ///     Logs a message indicating that the service is fetching the weather.
    /// </summary>
    [LoggerMessage(200, LogLevel.Information, "Fetching weather...")]
    partial void LogFetchingWeather();

    /// <summary>
    ///     Logs a message indicating that the service has successfully fetched the weather.
    /// </summary>
    [LoggerMessage(201, LogLevel.Information, "Fetched weather successfully")]
    partial void LogFetchWeatherSuccess();

    /// <summary>
    ///     Converts an array of temperatures from Celsius to Fahrenheit if the unit of measure specified in the options is
    ///     Celsius.
    /// </summary>
    /// <param name="temperatures">The array of temperatures to convert.</param>
    /// <returns>The converted array of temperatures.</returns>
    private int[] ConvertToFahrenheit(int[] temperatures)
    {
        if (!_options.Value.Unit.Equals("C", StringComparison.OrdinalIgnoreCase)) return temperatures;

        // If using Fahrenheit then convert the temperatures.
        for (var i = 0; i < temperatures.AsSpan().Length; i++)
            temperatures[i] = (int)Math.Round((temperatures[i] - 32) / 1.8);

        return temperatures;
    }
}