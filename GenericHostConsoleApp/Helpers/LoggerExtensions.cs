using Microsoft.Extensions.Logging;

namespace GenericHostConsoleApp.Helpers;

/// <summary>
///     Contains logger messages used throughout the application.
/// </summary>
public static partial class LoggerExtensions
{
    /// <summary>
    ///     Logs the details of an unhandled exception.
    /// </summary>
    /// <param name="ex">The unhandled exception.</param>
    /// <param name="logger">The <see cref="ILogger{TCategoryName}" /> to use.</param>
    [LoggerMessage(1, LogLevel.Critical, "An unhandled exception has occurred")]
    public static partial void LogUnhandledException(this ILogger logger, Exception ex);

    /// <summary>
    ///     Logs a message indicating that the application was cancelled.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger{TCategoryName}" /> to use.</param>
    [LoggerMessage(5, LogLevel.Information, "Application was cancelled")]
    public static partial void LogApplicationCancelled(this ILogger logger);

    /// <summary>
    ///     Logs a message indicating that the application is starting.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger{TCategoryName}" /> to use.</param>
    [LoggerMessage(6, LogLevel.Information, "Application is starting")]
    public static partial void LogApplicationStarting(this ILogger logger);

    /// <summary>
    ///     Logs a message indicating that the application has started.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger{TCategoryName}" /> to use.</param>
    /// <param name="args">The command line arguments.</param>
    [LoggerMessage(7, LogLevel.Information, "Application has started with args: \"{Args}\"")]
    public static partial void LogApplicationStarted(this ILogger logger, string[] args);

    /// <summary>
    ///     Logs a message indicating that the application is stopping.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger{TCategoryName}" /> to use.</param>
    [LoggerMessage(8, LogLevel.Information, "Application is stopping")]
    public static partial void LogApplicationStopping(this ILogger logger);

    /// <summary>
    ///     Logs a message indicating that the application has stopped.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger{TCategoryName}" /> to use.</param>
    [LoggerMessage(9, LogLevel.Information, "Application has stopped")]
    public static partial void LogApplicationStopped(this ILogger logger);

    /// <summary>
    ///     Logs a message indicating that the application is exiting.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger{TCategoryName}" /> to use.</param>
    /// <param name="exitCode">The exit code.</param>
    /// <param name="exitCodeAsInt">The exit code as an <see cref="int" />.</param>
    [LoggerMessage(10, LogLevel.Information, "Application is exiting with exit code: {ExitCode} ({ExitCodeAsInt})")]
    public static partial void LogApplicationExiting(this ILogger logger, ExitCode exitCode, int exitCodeAsInt);

    /// <summary>
    ///     Logs the details of an aggregate exception.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger" /> to use.</param>
    /// <param name="ex">The aggregate exception.</param>
    [LoggerMessage(12, LogLevel.Critical, "An unhandled exception has occurred")]
    public static partial void LogAggregateException(this ILogger logger, Exception ex);
}