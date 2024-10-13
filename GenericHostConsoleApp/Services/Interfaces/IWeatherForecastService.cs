namespace GenericHostConsoleApp.Services.Interfaces;

public interface IWeatherForecastService
{
    Task<string> FetchWeatherForecastAsync(string city, CancellationToken cancellationToken);
}