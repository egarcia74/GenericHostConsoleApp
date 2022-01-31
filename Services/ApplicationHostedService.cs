using GenericHostConsoleApp.Services.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GenericHostConsoleApp.Services;

/// <summary>
/// Hosted service that handles the application lifetime events and invokes the main application service.
/// </summary>
public sealed partial class ApplicationHostedService : IHostedService
{
    // Service dependencies.
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly ILogger<ApplicationHostedService> _logger;
    private readonly IMainService _mainService;

    /// <summary>
    /// Logs the details of an unhandled exception.
    /// </summary>
    /// <param name="ex">The unhandled exception.</param>
    [LoggerMessage(1, LogLevel.Critical, "An unhandled exception has occurred.")]
    partial void LogUnhandledException(Exception ex);

    /// <summary>
    /// Logs a message indicating that the application was cancelled.
    /// </summary>
    [LoggerMessage(2, LogLevel.Information, "Application was cancelled.")]
    partial void LogApplicationCancelled();

    /// <summary>
    /// Logs a message indicating that the application is starting.
    /// </summary>
    [LoggerMessage(3, LogLevel.Information, "Application is starting.")]
    partial void LogApplicationStarting();

    /// <summary>
    /// Logs a message indicating that the application has started.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    [LoggerMessage(4, LogLevel.Information, "Application has started with args: \"{Args}\".")]
    partial void LogApplicationStarted(string[] args);

    /// <summary>
    /// Logs a message indicating that the application is stopping.
    /// </summary>
    [LoggerMessage(5, LogLevel.Information, "Application is stopping.")]
    partial void LogApplicationStopping();

    /// <summary>
    /// Logs a message indicating that the application has stopped.
    /// </summary>
    [LoggerMessage(6, LogLevel.Information, "Application has stopped.")]
    partial void LogApplicationStopped();

    /// <summary>
    /// Logs a message indicating that the application is exiting.
    /// </summary>
    /// <param name="exitCode">The exit code.</param>
    /// <param name="exitCodeAsInt">The exit code as an <see cref="int"/>.</param>
    [LoggerMessage(7, LogLevel.Information, "Application is exiting with exit code: {ExitCode} ({ExitCodeAsInt}).")]
    partial void LogApplicationExiting(ExitCode exitCode, int exitCodeAsInt);

    // Cancellation token source used to submit a cancellation request.
    private CancellationTokenSource? _cancellationTokenSource;

    // Task that executes the main service.
    private Task<ExitCode>? _mainTask;

    /// <summary>
    /// Initialises the <see cref="ApplicationHostedService" /> using the specified dependencies.
    /// </summary>
    /// <param name="hostApplicationLifetime">Notifies of application lifetime events.</param>
    /// <param name="logger">The logger to use within this service.</param>
    /// <param name="mainService">The main service.</param>
    public ApplicationHostedService(
        IHostApplicationLifetime hostApplicationLifetime,
        ILogger<ApplicationHostedService> logger,
        IMainService mainService)
    {
        _hostApplicationLifetime = hostApplicationLifetime;
        _logger = logger;
        _mainService = mainService;
    }

    /// <summary>
    /// Triggered when the application host is ready to start the service.
    /// </summary>
    /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        LogApplicationStarting();

        // Initialise the cancellation token source so that it is in the cancelled state if the
        // supplied token is in the cancelled state.
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        // Bail if the start process has been aborted.
        if (_cancellationTokenSource.IsCancellationRequested) return Task.CompletedTask;

        //
        // Register callbacks to handle the application lifetime events.
        //

        // ApplicationStarted
        _hostApplicationLifetime.ApplicationStarted.Register(() =>
        {
            var args = Environment.GetCommandLineArgs();

            LogApplicationStarted(args);

            // Execute the main service but do not await it here. The task will be awaited in StopAsync().
            _mainTask = ExecuteMainAsync(args, _cancellationTokenSource.Token);
        });

        // ApplicationStopping
        _hostApplicationLifetime.ApplicationStopping.Register(LogApplicationStopping);

        // ApplicationStopped
        _hostApplicationLifetime.ApplicationStopped.Register(LogApplicationStopped);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Triggered when the application host is performing a graceful shutdown.
    /// </summary>
    /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        ExitCode exitCode;

        if (_mainTask is not null)
        {
            // If the main service is still running or the passed in cancellation token is in the cancelled state
            // then request a cancellation.
            if (!_mainTask.IsCompleted || cancellationToken.IsCancellationRequested)
                _cancellationTokenSource?.Cancel();

            // Wait for the main service to fully complete any cleanup tasks.
            // Note that this relies on the cancellation token to be properly used in the application.
            exitCode = await _mainTask.ConfigureAwait(false);
        }
        else
        {
            // The main service task never started.
            exitCode = ExitCode.Aborted;
        }

        Environment.ExitCode = (int)exitCode;

        LogApplicationExiting(exitCode, (int)exitCode);
    }

    /// <summary>
    /// Executes the <see cref="MainService.Main"/> method of the <see cref="MainService"/> and handles any exceptions.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// An <see cref="ExitCode"/> that indicates the outcome of the application..
    /// </returns>
    private async Task<ExitCode> ExecuteMainAsync(string[] args, CancellationToken cancellationToken)
    {
        try
        {
            return await _mainService.Main(args, cancellationToken);
        }
        catch (TaskCanceledException)
        {
            // This means the application is shutting down, so just swallow this exception.
            LogApplicationCancelled();

            return ExitCode.Cancelled;
        }
        catch (Exception ex)
        {
            LogUnhandledException(ex);

            return ExitCode.UnhandledException;
        }
        finally
        {
            // Stop the application once the work is done.
            _hostApplicationLifetime.StopApplication();
        }
    }
}