namespace GenericHostConsoleApp.Exceptions;

/// <summary>
///     Represents an exception that occurs during weather forecast retrieval.
/// </summary>
// ReSharper disable UnusedMember.Global
public class WeatherForecastException : Exception
{
    public WeatherForecastException()
    {
    }

    public WeatherForecastException(string message) : base(message)
    {
    }

    public WeatherForecastException(string message, Exception innerException) : base(message, innerException)
    {
    }
}