namespace GenericHostConsoleApp.Services.Interfaces;

/// <summary>
/// Fictional weather service interface.
/// </summary>
public interface IWeatherService
{
    /// <summary>
    /// Fictional weather service method.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A 5-day weather forecast.</returns>
    Task<IReadOnlyList<int>> GetFiveDayTemperaturesAsync(CancellationToken cancellationToken);
}