using System.ComponentModel.DataAnnotations;

namespace GenericHostConsoleApp.Configuration;

/// <summary>
///     Represents the options for the WeatherForecastService.
/// </summary>
// ReSharper disable UnusedAutoPropertyAccessor.Global
public class WeatherForecastServiceOptions
{
    /// <summary>
    ///     Represents the API key used for authentication in the WeatherForecastService.
    /// </summary>
    [Required]
    public string? ApiKey { get; set; }

    /// <summary>
    ///     Represents the options for the WeatherForecastService.
    /// </summary>
    [Required]
    public string? Url { get; set; }
}