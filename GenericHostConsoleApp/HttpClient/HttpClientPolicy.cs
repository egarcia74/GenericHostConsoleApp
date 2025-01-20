using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;

namespace GenericHostConsoleApp.HttpClient;

/// <summary>
///     Provides a set of Polly-based policies for handling transient errors, implementing circuit breaker patterns, and
///     managing timeouts in HTTP-based operations.
/// </summary>
public static class HttpClientPolicy
{
    /// <summary>
    ///     Creates and returns a Polly retry policy for handling transient HTTP errors.
    ///     The policy retries failed HTTP requests with exponential backoff and supports customizable retry logic.
    /// </summary>
    /// <param name="retryCount">The number of retry attempts to execute before failing. Defaults to 3 if not provided.</param>
    /// <param name="sleepDurationProvider">
    ///     Optional user-defined action invoked between retries. Provides details on the outcome, wait duration, retry
    ///     attempt, and execution context.
    /// </param>
    /// <returns>An asynchronous retry policy configured to handle transient HTTP errors and retry specified failures.</returns>
    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(
        int retryCount = 3,
        Action<DelegateResult<HttpResponseMessage>, TimeSpan, int, Context>? sleepDurationProvider = null)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError() // Handles 5xx and 408 status codes
            .OrResult(msg => (int)msg.StatusCode == 429) // Handle Too Many Requests (429)
            .WaitAndRetryAsync(retryCount, retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential backoff
                sleepDurationProvider);
    }

    /// <summary>
    ///     Creates and returns a Polly circuit breaker policy for handling transient HTTP errors and timeout exceptions.
    ///     The policy breaks the circuit after a specified number of consecutive failures and remains open for a defined
    ///     duration.
    /// </summary>
    /// <param name="handledEventsAllowedBeforeBreaking">
    ///     The number of consecutive failures to allow before breaking the circuit. Defaults to 5 if not provided.
    /// </param>
    /// <param name="durationOfBreak">
    ///     The duration of time the circuit will remain open before resetting. Defaults to 30 seconds if not provided.
    /// </param>
    /// <param name="onBreak">
    ///     An optional callback executed when the circuit transitions to an open state.
    /// </param>
    /// <param name="onReset">
    ///     An optional callback executed to modify context information when handling circuit breaker events.
    /// </param>
    /// <returns>An asynchronous circuit breaker policy for managing transient HTTP errors and timeout exceptions.</returns>
    public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(
        int handledEventsAllowedBeforeBreaking = 5,
        TimeSpan durationOfBreak = default,
        Action<DelegateResult<HttpResponseMessage>, TimeSpan, Context>? onBreak = null,
        Action<Context>? onReset = null)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError() // Handles 5xx and 408 status codes
            .Or<TimeoutRejectedException>() // Add timeout exceptions to the circuit breaker
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking,
                durationOfBreak == TimeSpan.Zero ? TimeSpan.FromSeconds(30) : durationOfBreak,
                onBreak,
                onReset);
    }

    /// <summary>
    ///     Creates and returns a Polly timeout policy for handling HTTP requests with specified timeouts.
    ///     The policy allows for handling timeout scenarios and defines fallback logic for handling timed-out requests.
    /// </summary>
    /// <param name="timeoutDuration">The maximum duration a request is allowed to run before timing out.</param>
    /// <param name="timeoutStrategy">The timeout strategy to apply, such as optimistic or pessimistic timeouts.</param>
    /// <param name="onTimeoutAsync">
    ///     A user-defined asynchronous callback invoked when a timeout occurs. Provides details about the context, timeout
    ///     duration, task, and exception.
    /// </param>
    /// <param name="fallbackAction">
    ///     A fallback action executed when a timeout is handled. This action returns an alternative HttpResponseMessage on
    ///     timeout.
    /// </param>
    /// <returns>
    ///     An asynchronous timeout policy configured to enforce a timeout and apply fallback logic for timed-out
    ///     requests.
    /// </returns>
    public static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy(
        TimeSpan timeoutDuration,
        TimeoutStrategy timeoutStrategy,
        Func<Context, TimeSpan, Task, Exception, Task> onTimeoutAsync,
        Func<CancellationToken, Task<HttpResponseMessage>> fallbackAction,
        Func<DelegateResult<HttpResponseMessage>, Task> onFallbackAsync)
    {
        return Policy<HttpResponseMessage>
            .Handle<TimeoutRejectedException>() // Handle timeout exceptions
            .FallbackAsync(fallbackAction, onFallbackAsync)
            .WrapAsync(
                Policy.TimeoutAsync<HttpResponseMessage>(
                    timeoutDuration,
                    timeoutStrategy,
                    onTimeoutAsync
                )
            );
    }
}