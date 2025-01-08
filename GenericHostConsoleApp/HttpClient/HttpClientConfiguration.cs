using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GenericHostConsoleApp.HttpClient;

/// <summary>
/// A static class that provides methods for configuring HTTP clients
/// with resilience and transient fault-handling policies using Polly integration.
/// </summary>
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMethodReturnValue.Local
public static class HttpClientConfiguration
{
    /// <summary>
    /// Registers HTTP clients with resilience and transient fault-handling policies using Polly.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the HTTP clients and policies to.</param>
    /// <param name="configuration">The application's <see cref="IConfiguration"/> to provide necessary settings for the HTTP client setup.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance so that multiple calls can be chained.</returns>
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IServiceCollection AddHttpClientsWithPolicies(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.ConfigureOpenWeatherHttpClient(configuration);

        return services;
    }

    /// <summary>
    /// Configures the OpenWeather HTTP client with a base address, default request headers,
    /// and applies resilience and transient fault-handling policies such as retry, circuit breaker, and timeout.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the OpenWeather HTTP client is added.</param>
    /// <param name="configuration">The <see cref="IConfiguration"/> used to retrieve the base address for the HTTP client.</param>
    private static IServiceCollection ConfigureOpenWeatherHttpClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Retrieve the BaseAddress for OpenWeatherHttpClient from the configuration
        var clientName = HttpClientName.OpenWeather.ToString();
        var baseAddress = configuration[$"HttpClients:{clientName}:BaseAddress"];

        if (string.IsNullOrWhiteSpace(baseAddress))
            throw new ArgumentNullException(nameof(configuration), $"{clientName} BaseAddress is not configured.");

        services
            .AddHttpClient(clientName, client =>
            {
                client.BaseAddress = new Uri(baseAddress);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            })
            .AddPolicyHandler(HttpPolicy.GetRetryPolicy())
            .AddPolicyHandler(HttpPolicy.GetCircuitBreakerPolicy())
            .AddPolicyHandler(HttpPolicy.GetTimeoutPolicy(TimeSpan.FromSeconds(10)));

        return services;
    }
}