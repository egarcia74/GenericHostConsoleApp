using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;

namespace GenericHostConsoleApp.HttpClient;

/// <summary>
/// Provides a set of Polly-based policies for handling transient errors, implementing circuit breaker patterns, and managing timeouts in HTTP-based operations.
/// </summary>
public static class HttpClientPolicy
{
    /// <summary>
    /// Creates and returns a Polly retry policy for handling transient HTTP errors.
    /// The policy retries failed HTTP requests with exponential backoff.
    /// </summary>
    /// <param name="retryCount">The number of retry attempts to execute before failing. Defaults to 3 if not provided.</param>
    /// <returns>An asynchronous retry policy for handling transient HTTP errors.</returns>
    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(int retryCount = 3)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError() // Handles 5xx and 408 status codes
            .OrResult(msg => (int)msg.StatusCode == 429) // Handle Too Many Requests (429)
            .WaitAndRetryAsync(retryCount, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))); // Exponential backoff
    }

    /// <summary>
    /// Creates and returns a Polly circuit breaker policy for handling transient HTTP errors and timeout exceptions.
    /// The policy breaks the circuit after a specified number of consecutive failures and remains open for a defined duration.
    /// </summary>
    /// <param name="handledEventsBeforeBreaking">The number of consecutive failures to allow before breaking the circuit. Defaults to 5 if not provided.</param>
    /// <param name="durationOfBreak">The duration of time the circuit will remain open before resetting. Defaults to 30 seconds if not provided.</param>
    /// <returns>An asynchronous circuit breaker policy for managing transient HTTP errors and timeout exceptions.</returns>
    public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(int handledEventsBeforeBreaking = 5,
        TimeSpan durationOfBreak = default)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError() // Handles 5xx and 408 status codes
            .Or<TimeoutRejectedException>() // Add timeout exceptions to the circuit breaker
            .CircuitBreakerAsync(handledEventsBeforeBreaking,
                durationOfBreak == TimeSpan.Zero ? TimeSpan.FromSeconds(30) : durationOfBreak);
    }

    /// <summary>
    /// Creates and returns a Polly timeout policy for HTTP operations.
    /// The policy cancels the request if it exceeds the specified timeout duration.
    /// </summary>
    /// <param name="timeoutDuration">The maximum allowed duration for the HTTP operation before timing out.</param>
    /// <returns>An asynchronous timeout policy for HTTP operations.</returns>
    public static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy(TimeSpan timeoutDuration)
    {
        return Policy.TimeoutAsync<HttpResponseMessage>(timeoutDuration);
    }
}