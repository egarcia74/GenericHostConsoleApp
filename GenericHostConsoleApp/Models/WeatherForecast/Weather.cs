using System.Text.Json.Serialization;

namespace GenericHostConsoleApp.Models.WeatherForecast;

// ReSharper disable once ClassNeverInstantiated.Global
public class Weather
{
    [JsonPropertyName("id")] public int Id { get; init; }

    [JsonPropertyName("main")] public string? Main { get; init; }

    [JsonPropertyName("description")] public string? Description { get; init; }

    [JsonPropertyName("icon")] public string? Icon { get; init; }
}