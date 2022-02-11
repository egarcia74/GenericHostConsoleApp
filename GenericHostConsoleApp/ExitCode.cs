namespace GenericHostConsoleApp;

/// <summary>
///     Exit codes used in this application.
/// </summary>
public enum ExitCode
{
    /// <summary>
    ///     Indicates that the application succeeded.
    /// </summary>
    Success = 0,

    /// <summary>
    ///     Indicates that the application was cancelled.
    /// </summary>
    Cancelled,

    /// <summary>
    ///     Indicates that there was an unhandled exception.
    /// </summary>
    UnhandledException,

    /// <summary>
    ///     Indicates that the application was aborted.
    /// </summary>
    Aborted
}