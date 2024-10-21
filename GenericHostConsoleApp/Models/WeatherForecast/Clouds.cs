using System.Text.Json.Serialization;

namespace GenericHostConsoleApp.Models.WeatherForecast;

// ReSharper disable once ClassNeverInstantiated.Global
public class Clouds
{
    [JsonPropertyName("all")] public int All { get; init; }
}