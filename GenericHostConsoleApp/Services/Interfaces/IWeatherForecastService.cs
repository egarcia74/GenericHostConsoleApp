namespace GenericHostConsoleApp.Services.Interfaces;

public interface IWeatherForecastService
{
    Task<string> FetchWeatherForecastAsync(CancellationToken cancellationToken);
}