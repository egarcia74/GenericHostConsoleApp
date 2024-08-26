namespace GenericHostConsoleApp.Exceptions;

/// <summary>
///     Represents an exception that occurs during weather forecast retrieval.
/// </summary>
public class WeatherForecastException(string message) : Exception(message);