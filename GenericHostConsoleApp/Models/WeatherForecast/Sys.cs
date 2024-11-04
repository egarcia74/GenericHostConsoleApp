using System.Text.Json.Serialization;

namespace GenericHostConsoleApp.Models.WeatherForecast;

// ReSharper disable once ClassNeverInstantiated.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
public class Sys
{
    [JsonPropertyName("type")] public int Type { get; init; }

    [JsonPropertyName("id")] public int Id { get; init; }

    [JsonPropertyName("country")] public string? Country { get; init; }

    [JsonPropertyName("sunrise")] public long Sunrise { get; init; }

    [JsonPropertyName("sunset")] public long Sunset { get; init; }
}