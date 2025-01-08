using System.ComponentModel.DataAnnotations;

namespace GenericHostConsoleApp.Configuration;

/// <summary>
///     Represents the options for the WeatherForecastService.
/// </summary>
// ReSharper disable UnusedAutoPropertyAccessor.Global
public record WeatherForecastServiceOptions
{
    /// <summary>
    ///     Represents the API key used for authentication in the WeatherForecastService.
    /// </summary>
    [Required]
    public required string ApiKey { get; init; }
}