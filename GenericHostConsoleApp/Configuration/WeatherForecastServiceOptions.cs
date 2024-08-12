using System.ComponentModel.DataAnnotations;

namespace GenericHostConsoleApp.Configuration;

public class WeatherForecastServiceOptions
{
    [Required] public string? ApiKey;
}