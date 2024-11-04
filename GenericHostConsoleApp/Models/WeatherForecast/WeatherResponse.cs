using System.Text.Json.Serialization;

namespace GenericHostConsoleApp.Models.WeatherForecast;

// ReSharper disable once ClassNeverInstantiated.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
public class WeatherResponse
{
    [JsonPropertyName("coord")] public Coordinates? Coord { get; init; }
    [JsonPropertyName("weather")] public List<Weather>? Weather { get; init; } = [];

    [JsonPropertyName("base")] public string? Base { get; init; }

    [JsonPropertyName("main")] public Main? Main { get; init; }

    [JsonPropertyName("visibility")] public int Visibility { get; init; }

    [JsonPropertyName("wind")] public Wind? Wind { get; init; }

    [JsonPropertyName("clouds")] public Clouds? Clouds { get; init; }

    [JsonPropertyName("dt")] public long Dt { get; init; }

    [JsonPropertyName("sys")] public Sys? Sys { get; init; }

    [JsonPropertyName("timezone")] public int Timezone { get; init; }

    [JsonPropertyName("id")] public int Id { get; init; }

    [JsonPropertyName("name")] public string? Name { get; init; }

    [JsonPropertyName("cod")] public int Cod { get; init; }
}