namespace GenericHostConsoleApp.Services.Interfaces;

/// <summary>
///     A service for fetching weather forecasts from an external API.
/// </summary>
public interface IWeatherForecastService
{
    /// <summary>
    ///     Fetches the weather forecast asynchronously.
    /// </summary>
    /// <param name="city">The city to get the forecast for.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The weather forecast as a string.</returns>
    Task<string> FetchWeatherForecastAsync(string city, CancellationToken cancellationToken);
}