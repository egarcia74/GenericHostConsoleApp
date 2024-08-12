using System.ComponentModel.DataAnnotations;

namespace GenericHostConsoleApp.Configuration;

public class WeatherForecastServiceOptions
{
    [Required] public string? ApiKey { get; set; }
    [Required] public string? Url { get; set; }
    [Required] public string? City { get; set; }
}