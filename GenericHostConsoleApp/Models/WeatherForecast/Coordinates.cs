using System.Text.Json.Serialization;

namespace GenericHostConsoleApp.Models.WeatherForecast;

// ReSharper disable once ClassNeverInstantiated.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
public record Coordinates
{
    [JsonPropertyName("lon")] public double Lon { get; init; }

    [JsonPropertyName("lat")] public double Lat { get; init; }
}