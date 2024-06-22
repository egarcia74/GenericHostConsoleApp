using System.ComponentModel.DataAnnotations;
using GenericHostConsoleApp.Services;

namespace GenericHostConsoleApp.Configuration;

/// <summary>
///     Configuration options for <see cref="WeatherService" />.
/// </summary>
public sealed class WeatherOptions
{
    // Indicates the unit of measure for the weather forecasts.
    [Required]
    [RegularExpression("^(?:C|F)$", ErrorMessage = "Unit must be either \"C\" or \"F\"")]
    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
    public required string Unit { get; init; } = string.Empty;
}