using System.Reflection;
using GenericHostConsoleApp.Configuration;
using GenericHostConsoleApp.Services;
using GenericHostConsoleApp.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

// Configure and start the application host. 
await Host.CreateDefaultBuilder(args)
    // Set the content root directory for the host instance.
    .UseContentRoot(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                    ?? throw new InvalidOperationException("Failed to get the path to the current assembly."))
    .ConfigureAppConfiguration((_, builder) =>
    {
        // Add user secrets to the configuration
        builder.AddUserSecrets<Program>(false, true);

        // Uncomment below code to add a command line configuration provider:
        builder.AddCommandLine(args, new Dictionary<string, string>
        {
            { "-c", "City" },
            { "--city", "City" }
        });
    })
    .ConfigureServices((hostContext, services) =>
    {
        // Add the needed services
        services
            .AddHostedService<ApplicationHostedService>()
            .AddTransient<IMainService, MainService>()
            .AddTransient<IWeatherForecastService, WeatherForecastService>()
            .AddHttpClient<WeatherForecastService>();

        // Set up the application options.
        services
            .AddOptions<WeatherForecastServiceOptions>()
            .Bind(hostContext.Configuration.GetSection(nameof(WeatherForecastServiceOptions)))
            .ValidateDataAnnotations()
            .ValidateOnStart();
    })
    // Add logging capabilities
    .UseSerilog((context, configuration) =>
        configuration
            .ReadFrom.Configuration(context.Configuration))

    // Start the host instance as a console application.
    .RunConsoleAsync();