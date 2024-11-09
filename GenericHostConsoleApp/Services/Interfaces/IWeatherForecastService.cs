using GenericHostConsoleApp.Models.WeatherForecast;

namespace GenericHostConsoleApp.Services.Interfaces;

/// <summary>
///     A service for fetching weather forecasts from an external API.
/// </summary>
public interface IWeatherForecastService
{
    /// <summary>
    ///     Fetches the weather forecast for a specified name from an external API asynchronously.
    /// </summary>
    /// <param name="name">The name of the place for which to fetch the weather forecast.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A Task representing the asynchronous operation, which upon completion contains the weather forecast response.</returns>
    Task<WeatherResponse> FetchWeatherForecastAsync(string name, CancellationToken cancellationToken);
}