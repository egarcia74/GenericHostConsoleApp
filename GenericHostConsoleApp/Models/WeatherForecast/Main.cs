using System.Text.Json.Serialization;

namespace GenericHostConsoleApp.Models.WeatherForecast;

// ReSharper disable once ClassNeverInstantiated.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
public record Main
{
    [JsonPropertyName("temp")] public double Temp { get; init; }

    [JsonPropertyName("feels_like")] public double FeelsLike { get; init; }

    [JsonPropertyName("temp_min")] public double TempMin { get; init; }

    [JsonPropertyName("temp_max")] public double TempMax { get; init; }

    [JsonPropertyName("pressure")] public int Pressure { get; init; }

    [JsonPropertyName("humidity")] public int Humidity { get; init; }

    [JsonPropertyName("sea_level")] public int SeaLevel { get; init; }

    [JsonPropertyName("grnd_level")] public int GroundLevel { get; init; }
}