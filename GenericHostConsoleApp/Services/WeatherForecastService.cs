using System.Net;
using System.Text.Json;
using GenericHostConsoleApp.Configuration;
using GenericHostConsoleApp.Exceptions;
using GenericHostConsoleApp.HttpClient;
using GenericHostConsoleApp.Models.WeatherForecast;
using GenericHostConsoleApp.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GenericHostConsoleApp.Services;

/// <summary>
///     A service for fetching weather forecasts from an external API.
/// </summary>
public class WeatherForecastService(
    IOptions<WeatherForecastServiceOptions> options,
    ILogger<WeatherForecastService> logger,
    IHttpClientFactory httpClientFactory)
    : IWeatherForecastService
{
    /// <summary>
    ///     Fetches the weather forecast for a specified name from an external API asynchronously.
    /// </summary>
    /// <param name="name">The name of the place for which to fetch the weather forecast.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <exception cref="WeatherForecastException">WeatherForecastServiceOptions are not properly configured.</exception>
    /// <returns>A Task representing the asynchronous operation, which upon completion contains the weather forecast response.</returns>
    public async Task<WeatherResponse> FetchWeatherForecastAsync(string name, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var httpClientName = HttpClientName.OpenWeather.ToString();
        var httpClient = httpClientFactory.CreateClient(httpClientName);

        const string relativePath = "/data/2.5/weather";

        var uriBuilder = new UriBuilder(new Uri(httpClient.BaseAddress!, relativePath))
        {
            Query = $"q={name}&appid={options.Value.ApiKey}"
        };

        var openWeatherUrl = uriBuilder.ToString();

        // Log the URL but obfuscate the key for security
        logger.LogDebug("OpenWeather Url: {Url}", openWeatherUrl.Replace(options.Value.ApiKey, "*****"));

        using var response = await httpClient.GetAsync(openWeatherUrl, cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
                throw new WeatherForecastException($"The name \"{name}\" was not found.");

            throw new WeatherForecastException(
                $"Failed to fetch weather data: Status: {response.StatusCode}; {response.Content}");
        }

        // Process the response
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            var weatherResponse = JsonSerializer.Deserialize<WeatherResponse>(responseContent);
            if (weatherResponse == null) throw new WeatherForecastException("Failed to deserialize weather response.");

            return weatherResponse;
        }
        catch (JsonException ex)
        {
            throw new WeatherForecastException("Failed to parse weather data.", ex);
        }
    }
}