using GenericHostConsoleApp.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace GenericHostConsoleApp.Services;

/// Represents a service for fetching weather forecasts.
public class WeatherForecastService(IConfiguration configuration, HttpClient httpClient) : IWeatherForecastService
{
    /// <summary>
    ///     Fetches the weather forecast asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The weather forecast as a string.</returns>
    public async Task<string> FetchWeatherForecastAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var url = configuration.GetSection("WeatherForecastServiceOptions:Url").Value;
        var apiKey = configuration.GetSection("WeatherForecastServiceOptions:ApiKey").Value;
        var city = configuration.GetSection("WeatherForecastServiceOptions:City").Value;

        var response = await httpClient.GetAsync($"{url}?q={city}&appid={apiKey}", cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Failed to fetch weather data: Status: {response.StatusCode}; {response.Content}");

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}