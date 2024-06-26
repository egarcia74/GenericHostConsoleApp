using System.Reflection;
using GenericHostConsoleApp.Configuration;
using GenericHostConsoleApp.Services;
using GenericHostConsoleApp.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Exceptions;

// Configure and start the application host. 
await Host.CreateDefaultBuilder(args)
    .UseContentRoot(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                    ?? throw new InvalidOperationException($"Failed to get the path to the current assembly."))
    .ConfigureAppConfiguration((_, builder) =>
    {
        builder.AddUserSecrets<Program>(true, true);

        // builder.AddCommandLine(args, new Dictionary<string, string>
        // {
        //     // Define parameter mappings.
        //     { "-a1", "arg1" },
        //     { "-a2", "arg2" }
        // });
    })
    .ConfigureServices((hostContext, services) =>
    {
        services
            .AddHostedService<ApplicationHostedService>()
            .AddTransient<IMainService, MainService>()
            .AddSingleton<IWeatherService, WeatherService>()
            .AddSingleton<IUserNotificationService, UserNotificationService>();

        services
            .AddOptions<WeatherOptions>()
            .Bind(hostContext.Configuration.GetSection(nameof(WeatherOptions)))
            .ValidateDataAnnotations()
            .ValidateOnStart();
    })
    .UseSerilog((context, configuration) => 
        configuration
            .ReadFrom.Configuration(context.Configuration))
    .RunConsoleAsync()
    .ConfigureAwait(false);