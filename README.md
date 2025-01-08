# GenericHostConsoleApp

A console app example
using [Host.CreateApplicationBuilder](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.hosting.host.createapplicationbuilder?view=net-9.0-pp).

This code is derived from [David Federman's](https://github.com/dfederm) original
code: https://github.com/dfederm/GenericHostConsoleApp

For more details, refer to his original [blog post](https://dfederm.com/building-a-console-app-with-.net-generic-host/).

This version adds a few extra bells and whistles such as:

* Separating the main application logic from the boilerplate startup code. There is now a separate MainService class,
  which can be easily modified to implement custom logic without having to worry about all the application plumbing
  required to wire up the application hosted service.
* Moved classes into subdirectories corresponding to the class' area of concern. E.g. Configuration, Services,
  Interfaces.
* Using an **ExitCode** enum to define return values.
* [Compile-time logging source generation](https://docs.microsoft.com/en-us/dotnet/core/extensions/logger-message-generator)
  .
* [Validation of configuration options](https://docs.microsoft.com/en-us/dotnet/core/extensions/options#options-validation)
  .
* [Serilog](https://serilog.net) as the logging provider.
* Unit tests using xUnit and Moq.
* This reference implementation uses a weather forecast service that fetches the weather from [Open Weather](https://openweathermap.org).

## Program.cs

```C#
using GenericHostConsoleApp.Configuration;
using GenericHostConsoleApp.Services;
using GenericHostConsoleApp.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

// Create the host builder
var builder = Host.CreateApplicationBuilder(args);

// Configure configuration sources
builder.Configuration.AddEnvironmentVariables();

// Add command-line arguments with mappings
builder.Configuration.AddCommandLine(args, new Dictionary<string, string>
{
    { "-n", "Name" },
    { "--name", "Name" }
});

// Configure logging
builder.Logging.ClearProviders(); // Remove default providers
builder.Services.AddSerilog((_, configuration) => 
        configuration.ReadFrom.Configuration(builder.Configuration));

// Configure services
builder.Services.AddHostedService<ApplicationHostedService>();
builder.Services.AddTransient<IMainService, MainService>();
builder.Services.AddTransient<IWeatherForecastService, WeatherForecastService>();
builder.Services.AddHttpClient<WeatherForecastService>();

// Configure options and validation
builder.Services
    .AddOptions<WeatherForecastServiceOptions>()
    .Bind(builder.Configuration.GetSection(nameof(WeatherForecastServiceOptions)))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Build and run the host
using var host = builder.Build();

try
{
    await host.RunAsync();
}
finally
{
    Log.CloseAndFlush();
}
```

This code snippet sets up a HostApplicationBuilder and adds a hosted service called ApplicationHostedService. This service will run in the background and perform tasks as defined in its implementation. You can reuse ApplicationHostedService to handle the application's lifecycle and background tasks.
This example also demonstrates how to use dependency injection to inject the IMainService and IWeatherForecastService dependencies into the ApplicationHostedService.
Running the Application

## Implementing the main application logic in IMainService.ExecuteMainAsync

The MainService class contains the main application logic. Here's an example of what the IMainService.ExecuteMainAsync method might look like:

```C#
public async Task ExecuteMainAsync(string[] args)
{
    // 1. Get the weather forecast
    var weatherForecast = await _weatherForecastService.GetWeatherForecastAsync(args);

    // 2. Log the weather forecast
    _logger.LogInformation("Weather forecast for {City}: {Forecast}", args[0], weatherForecast);

    // 3. Do something with the weather forecast...
}
```

This method retrieves the weather forecast from the IWeatherForecastService, logs it, and then performs some action with the data. You can modify this method to implement your own application logic.

## Notes:
* When you run the project in the Development environment (`DOTNET_ENVIRONMENT=Development`), be sure to specify your [Open Weather](https://openweathermap.org) API key in a .NET User Secrets file.

```
{
  "WeatherForecastServiceOptions": {
    "ApiKey": "123456789123456789"
  }
}
```

When running in the Production environment (`DOTNET_ENVIRONMENT=Production`), you can specify the API key by setting the following environment variable:

```
WeatherForecastServiceOptions__ApiKey=123456789123456789
```

* To run, specify the name of the place to get the weather forecast using the command line as follows:

```
dotnet run -- --name Berlin
```

If you want to use .NET User Secrets, you might want to specify the environment name either via the DOTNET_ENVIRONMENT variable, or via the --environment option of dotnet:

```
dotnet run --environment "Development" -- --name Berlin
```

The output should look as follows:
```
[01/08/2025 21:18:28 +10:00 Information] Weather forecast for "Berlin", "DE": Temperature: 4"ºC" (feels like -1"ºC"), Min: 2"ºC", Max: 4"ºC". Weather: "few clouds" {SourceContext="GenericHostConsoleApp.Services.MainService", ThreadId=10}
```

[![.NET](https://github.com/egarcia74/GenericHostConsoleApp/actions/workflows/dotnet.yml/badge.svg)](https://github.com/egarcia74/GenericHostConsoleApp/actions/workflows/dotnet.yml)
