using System.Diagnostics;
using System.Text.Json;
using GenericHostConsoleApp.Configuration;
using GenericHostConsoleApp.Exceptions;
using GenericHostConsoleApp.Models.WeatherForecast;
using GenericHostConsoleApp.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Timeout;
using Polly.Wrap;

namespace GenericHostConsoleApp.Services;

/// <summary>
///     A service for fetching weather forecasts from an external API.
/// </summary>
public class WeatherForecastService(
    HttpClient httpClient,
    IOptions<WeatherForecastServiceOptions> options,
    ILogger<WeatherForecastService> logger)
    : IWeatherForecastService
{
    /// <summary>
    /// Fetches the weather forecast for a specified city from an external API asynchronously.
    /// </summary>
    /// <param name="city">The name of the city for which to fetch the weather forecast.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A Task representing the asynchronous operation, which upon completion contains the weather forecast response.</returns>
    public async Task<WeatherResponse> FetchWeatherForecastAsync(string city, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var url = $"{options.Value.Url}?q={city}&appid={options.Value.ApiKey}";

        // Log the URL but obfuscate the key for security
        Debug.Assert(options.Value.ApiKey != null, "options.Value.ApiKey != null");
        logger.LogInformation("OpenWeather Url: {Url}", url.Replace(options.Value.ApiKey, "*****"));

        var policyWrap = GetPolicy();

        var response = await policyWrap.ExecuteAsync(token => httpClient.GetAsync(url, token), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new WeatherForecastException(
                $"Failed to fetch weather data: Status: {response.StatusCode}; {response.Content}"); // Use a more specific exception type

        // Process the response
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        try
        {
            var weatherResponse = JsonSerializer.Deserialize<WeatherResponse>(responseContent);
            if (weatherResponse == null)
            {
                throw new WeatherForecastException("Failed to deserialize weather response.");
            }

            return weatherResponse;
        }
        catch (JsonException ex)
        {
            throw new WeatherForecastException("Failed to parse weather data.", ex);
        }
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