using System.Text.Json.Serialization;

namespace GenericHostConsoleApp.Models.WeatherForecast;

public class Clouds
{
    [JsonPropertyName("all")] public int All { get; set; }
}