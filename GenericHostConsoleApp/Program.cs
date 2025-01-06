﻿using GenericHostConsoleApp.Configuration;
using GenericHostConsoleApp.Services;
using GenericHostConsoleApp.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

// Create the host builder
var builder = Host.CreateApplicationBuilder(args);

// Configure configuration sources
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddEnvironmentVariables();

// Add command-line arguments with mappings
builder.Configuration.AddCommandLine(args, new Dictionary<string, string>
{
    { "-n", "Name" },
    { "--name", "Name" }
});

// Configure logging
builder.Services
    .AddSerilog((_, configuration) => configuration
    .ReadFrom.Configuration(builder.Configuration) 
    .Enrich.FromLogContext()
    .WriteTo.Console());

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
catch (Exception ex)
{
    // Log the exception and rethrow it
    Log.Fatal(ex, "Host terminated unexpectedly");
    throw;
}
finally
{
    // Ensure the host is disposed
    Log.CloseAndFlush();
}