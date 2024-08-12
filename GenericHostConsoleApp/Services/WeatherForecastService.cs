using GenericHostConsoleApp.Configuration;
using GenericHostConsoleApp.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GenericHostConsoleApp.Services;

/// Represents a service for fetching weather forecasts.
public class WeatherForecastService(HttpClient httpClient, IOptions<WeatherForecastServiceOptions> options, ILogger<WeatherForecastService> logger)
    : IWeatherForecastService
{
    /// <summary>
    ///     Fetches the weather forecast asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The weather forecast as a string.</returns>
    public async Task<string> FetchWeatherForecastAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var url = $"{options.Value.Url}?q={options.Value.City}&appid={options.Value.ApiKey}";
        
        logger.LogInformation("OpenWeather Url: {url}", url);

        var response = await httpClient.GetAsync(url, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Failed to fetch weather data: Status: {response.StatusCode}; {response.Content}");

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}