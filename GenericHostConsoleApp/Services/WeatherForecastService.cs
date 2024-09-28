using System.Diagnostics;
using GenericHostConsoleApp.Configuration;
using GenericHostConsoleApp.Exceptions;
using GenericHostConsoleApp.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Timeout;
using Polly.Wrap;

namespace GenericHostConsoleApp.Services;

/// Represents a service for fetching weather forecasts.
public class WeatherForecastService(
    IConfiguration config,
    HttpClient httpClient,
    IOptions<WeatherForecastServiceOptions> options,
    ILogger<WeatherForecastService> logger)
    : IWeatherForecastService
{
    /// <summary>
    ///     Fetches the weather forecast asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The weather forecast as a string.</returns>
    public async Task<string> FetchWeatherForecastAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var url = $"{options.Value.Url}?q={config["City"]}&appid={options.Value.ApiKey}";

        // Log the URL but obfuscate the key for security
        Debug.Assert(options.Value.ApiKey != null, "options.Value.ApiKey != null");
        logger.LogInformation("OpenWeather Url: {Url}", url.Replace(options.Value.ApiKey, "*****"));

        var policyWrap = GetPolicy();

        var response = await policyWrap.ExecuteAsync(token => httpClient.GetAsync(url, token), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new WeatherForecastException(
                $"Failed to fetch weather data: Status: {response.StatusCode}; {response.Content}"); // Use a more specific exception type

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    /// <summary>
    ///     Retrieves the policy for handling retries, circuit breaking, and timeouts in the weather forecast service.
    /// </summary>
    /// <returns>
    ///     The policy wrap that combines the retry, circuit breaker, and timeout policies.
    /// </returns>
    private AsyncPolicyWrap GetPolicy()
    {
        var retryPolicy = Policy
            .Handle<HttpRequestException>()
            .WaitAndRetryAsync(6,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (exception, retryCount, context) =>
                {
                    // ReSharper disable once ExceptionPassedAsTemplateArgumentProblem
                    logger.LogWarning("Retry {RetryCount} for policy: {ContextPolicyKey}, due to: {Exception}",
                        retryCount, context.PolicyKey, exception);
                });

        var circuitBreakerPolicy = Policy
            .Handle<HttpRequestException>()
            .CircuitBreakerAsync(5, TimeSpan.FromMinutes(1),
                (exception, breakDelay) =>
                    logger.LogWarning("Circuit breaker opened due to: {Exception}. Break for: {BreakDelay}", exception,
                        breakDelay),
                () => logger.LogInformation("Circuit breaker reset"));

        var timeoutPolicy = Policy.TimeoutAsync(30,
            TimeoutStrategy.Optimistic, // More appropriate for HttpClient
            (context, timeSpan, _) =>
            {
                logger.LogWarning(
                    "Timeout from policy: {ContextPolicyKey} after waiting {TimeSpanTotalSeconds} seconds",
                    context.PolicyKey, timeSpan.TotalSeconds);

                return Task.CompletedTask;
            });

        return Policy.WrapAsync(retryPolicy, circuitBreakerPolicy, timeoutPolicy);
    }
}