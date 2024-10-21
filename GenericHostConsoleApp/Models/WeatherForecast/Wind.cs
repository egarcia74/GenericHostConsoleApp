using System.Text.Json.Serialization;

namespace GenericHostConsoleApp.Models.WeatherForecast;

// ReSharper disable once ClassNeverInstantiated.Global
public class Wind
{
    [JsonPropertyName("speed")] public double Speed { get; init; }

    [JsonPropertyName("deg")] public int Deg { get; init; }
}