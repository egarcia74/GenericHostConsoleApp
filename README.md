# GenericHostConsoleApp

A console app example
using [.NET Generic Host](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host).

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

## Example Usage

This project demonstrates how to use the .NET Generic Host to create a long-running console application. Here's a basic example of how to get started:

```C#
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<ApplicationHostedService>();
        services.AddTransient<IMainService, MainService>(); 
        services.AddTransient<IWeatherForecastService, WeatherForecastService>();
    })
    .Build();

await host.RunAsync();

This code snippet sets up a Generic Host and adds a hosted service called ApplicationHostedService. This service will run in the background and perform tasks as defined in its implementation. You can reuse ApplicationHostedService to handle the application's lifecycle and background tasks.
This example also demonstrates how to use dependency injection to inject the IMainService and IWeatherForecastService dependencies into the ApplicationHostedService.
Running the Application

Notes:
* Be sure to specify your [Open Weather](https://openweathermap.org) API key in a .NET User Secrets file:

```
{
  "WeatherForecastServiceOptions": {
    "ApiKey": "123456789123456789"
  }
}
```

* To run, specify the name of the place to get the weather forecast using the command line as follows:
```
dotnet run -- --name Berlin
```

[![.NET](https://github.com/egarcia74/GenericHostConsoleApp/actions/workflows/dotnet.yml/badge.svg)](https://github.com/egarcia74/GenericHostConsoleApp/actions/workflows/dotnet.yml)

https://github.com/melnkovmaxim/Discord.Bot
 * https://github.com/draghav/Airtek