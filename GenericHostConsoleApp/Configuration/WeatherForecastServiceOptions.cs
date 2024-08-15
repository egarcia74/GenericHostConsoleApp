using System.ComponentModel.DataAnnotations;

namespace GenericHostConsoleApp.Configuration;

/// <summary>
/// Represents the options for the WeatherForecastService.
/// </summary>
public class WeatherForecastServiceOptions
{
    /// <summary>
    /// Represents the API key used for authentication in the WeatherForecastService.
    /// </summary>
    [Required] public string? ApiKey { get; set; }

    /// <summary>
    /// Represents the options for the WeatherForecastService.
    /// </summary>
    [Required] public string? Url { get; set; }

    /// <summary>
    /// Represents the city used for fetching weather forecasts in the WeatherForecastService.
    /// </summary>
    [Required] public string? City { get; set; }
}