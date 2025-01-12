using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly.Timeout;

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
        services.AddOpenWeatherHttpClient(configuration);

        return services;
    }

    /// <summary>
    /// Configures the OpenWeather HTTP client with a base address, default request headers,
    /// and applies resilience and transient fault-handling policies such as retry, circuit breaker, and timeout.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the OpenWeather HTTP client is added.</param>
    /// <param name="configuration">The <see cref="IConfiguration"/> used to retrieve the base address for the HTTP client.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance to allow method chaining.</returns>
    private static IServiceCollection AddOpenWeatherHttpClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Retrieve the BaseAddress for OpenWeatherHttpClient from the configuration
        const string clientName = nameof(HttpClientName.OpenWeather);

        var baseAddress = configuration.GetValue<string>($"HttpClients:{clientName}:BaseAddress");

        if (string.IsNullOrWhiteSpace(baseAddress))
            throw new ArgumentNullException(nameof(configuration), $"{clientName} BaseAddress is not configured.");

        var retries = configuration.GetValue<int?>($"HttpClients:{clientName}:Retries") ?? 5;

        var handledEventsBeforeBreaking =
            configuration.GetValue<int?>($"HttpClients:{clientName}:EventsBeforeBreaking") ?? 5;

        var durationOfBreak =
            configuration.GetValue<TimeSpan?>($"HttpClients:{clientName}:DurationOfBreak") ??
            TimeSpan.FromMinutes(5);

        var timeout =
            configuration.GetValue<TimeSpan?>($"HttpClients:{clientName}:Timeout") ??
            TimeSpan.FromMinutes(5);

        var logger = services.BuildServiceProvider().GetRequiredService<ILogger<System.Net.Http.HttpClient>>();

        services
            .AddHttpClient(clientName, client =>
            {
                client.BaseAddress = new Uri(baseAddress);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            })
            .AddPolicyHandler(HttpClientPolicy.GetRetryPolicy(
                retries,
                sleepDurationProvider: (outcome, timespan, retryAttempt, context) =>
                {
                    logger.LogWarning(
                        "Policy {ContextPolicyKey} retry {RetryAttempt} for {ClientName}. Waiting {TotalSeconds} seconds. Exception: {ExceptionMessage}",
                        context.PolicyKey, 
                        retryAttempt, 
                        clientName, 
                        timespan.TotalSeconds, 
                        outcome.Exception?.Message);
                }))
            .AddPolicyHandler(HttpClientPolicy.GetCircuitBreakerPolicy(handledEventsBeforeBreaking, durationOfBreak,
                onBreak: (response, timespan, context) =>
                {
                    logger.LogWarning(
                        "Policy {ContextPolicyKey} circuit broken for {TotalSeconds} seconds. Exception: {ExceptionMessage}",
                        context.PolicyKey, 
                        timespan.TotalSeconds, 
                        response.Exception.Message);
                },
                onReset: context =>
                {
                    logger.LogInformation("Policy {ContextPolicyKey} circuit reset", context.PolicyKey);
                }))
            .AddPolicyHandler(HttpClientPolicy.GetTimeoutPolicy(timeout, TimeoutStrategy.Pessimistic,
                onTimeoutAsync: (context, timespan, _, exception) =>
                {
                    // Log the timeout exception here if needed (optional)
                    logger.LogWarning(
                        "Policy {ContextPolicyKey} timeout occurred after {TotalSeconds} seconds. Exception: {ExceptionMessage}", 
                        context.PolicyKey,
                        timespan.TotalSeconds,
                        exception.Message);
                    
                    return Task.CompletedTask;
                },
                fallbackAction: async (cancellationToken) =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                        
                    logger.LogWarning("Request timed out after {TotalSeconds}", timeout.TotalSeconds);

                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent("The request timed out.")
                    };
                }));

        return services;
    }
}