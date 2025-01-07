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