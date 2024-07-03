using GenericHostConsoleApp.Helpers;
using GenericHostConsoleApp.Services.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GenericHostConsoleApp.Services;

/// <summary>
/// Represents a hosted service in an ASP.NET Core application or a .NET Core worker.
/// A hosted service is a service that runs long-running background tasks, which typically run over the lifetime of your application.
/// </summary>
public sealed class ApplicationHostedService : IHostedService
{
    // Service dependencies.
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly ILogger<ApplicationHostedService> _logger;
    private readonly IMainService _mainService;

    // Cancellation token source used to submit a cancellation request.
    private CancellationTokenSource? _cancellationTokenSource;

    // Task that executes the main service.
    private Task<ExitCode>? _mainTask;

    /// <summary>
    ///     Initialises the <see cref="ApplicationHostedService" /> using the specified dependencies.
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
    ///     Triggered when the application host is ready to start the service.
    /// </summary>
    /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogApplicationStarting();

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

            _logger.LogApplicationStarted(args);

            // Execute the main service but do not await it here. The task will be awaited in StopAsync().
            _mainTask = ExecuteMainAsync(args, _cancellationTokenSource.Token);
        });

        // ApplicationStopping
        _hostApplicationLifetime.ApplicationStopping.Register(_logger.LogApplicationStopping);

        // ApplicationStopped
        _hostApplicationLifetime.ApplicationStopped.Register(_logger.LogApplicationStopped);

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Triggered when the application host is performing a graceful shutdown.
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
                _cancellationTokenSource?
                    .CancelAsync()
                    .ConfigureAwait(false);

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

        _logger.LogApplicationExiting(exitCode, (int)exitCode);
    }    
    private async Task<ExitCode> ExecuteMainAsync(string[] args, CancellationToken cancellationToken)
    {
        try
        {
            return await _mainService.Main(args, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
        finally
        {
            // Stop the application once the work is done.
            _hostApplicationLifetime.StopApplication();
        }
    }
    
    /// <summary>
    /// Handles the given exception, logs it accordingly, and determines the appropriate exit code.
    /// </summary>
    /// <param name="ex">The exception that was thrown</param>
    /// <returns>The corresponding exit code</returns>
    private ExitCode HandleException(Exception ex)
    {
        switch (ex)
        {
            case TaskCanceledException:
                _logger.LogApplicationCancelled();
                return ExitCode.Cancelled;

            case ArgumentNullException argumentNullException:
                _logger.LogArgumentNullException(argumentNullException);
                return ExitCode.ArgumentNullException;

            case ArgumentException argumentException:
                _logger.LogArgumentException(argumentException);
                return ExitCode.ArgumentException;

            case InvalidOperationException invalidOperationException:
                _logger.LogInvalidOperationException(invalidOperationException);
                return ExitCode.InvalidOperationException;

            default:
                _logger.LogUnhandledException(ex);
                return ExitCode.UnhandledException;
        }
    }
}