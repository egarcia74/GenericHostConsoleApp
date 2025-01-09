namespace GenericHostConsoleApp.Helpers;

/// <summary>
///     A static utility class for converting temperature values between different units.
/// </summary>
public static class TemperatureConverter
{
    /// <summary>
    ///     Converts a temperature value from one temperature unit to another.
    /// </summary>
    /// <param name="value">The temperature value to convert.</param>
    /// <param name="fromUnit">The unit of the temperature value to convert from.</param>
    /// <param name="toUnit">The unit of the temperature value to convert to.</param>
    /// <exception cref="InvalidOperationException">Unsupported temperature conversion requested.</exception>
    /// <returns>The converted temperature value.</returns>
    public static double ConvertTemperature(double value, TemperatureUnit fromUnit, TemperatureUnit toUnit)
    {
        if (fromUnit == toUnit) return value;

        // Convert from the source unit to Celsius as an intermediate step
        var valueInCelsius = fromUnit switch
        {
            TemperatureUnit.Celsius => value,
            TemperatureUnit.Fahrenheit => (value - 32) * 5 / 9,
            TemperatureUnit.Kelvin => value - 273.15,
            _ => throw new InvalidOperationException("Unsupported temperature conversion requested.")
        };

        // Convert from Celsius to the target unit
        return toUnit switch
        {
            TemperatureUnit.Celsius => valueInCelsius,
            TemperatureUnit.Fahrenheit => valueInCelsius * 9 / 5 + 32,
            TemperatureUnit.Kelvin => valueInCelsius + 273.15,
            _ => throw new InvalidOperationException("Unsupported temperature conversion requested.")
        };
    }
}