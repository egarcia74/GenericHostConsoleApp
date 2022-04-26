// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global
namespace GenericHostConsoleApp.Helpers
{
    /// <summary>
    /// The result of an operation that does not produce a value.
    /// </summary>
    public class Result
    {
        /// <summary>
        /// Indicates whether the operation succeeded.
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// The exception that occurred in the case where the operation failed.
        /// </summary>
        public Exception? Exception { get; }

        /// <summary>
        /// An object providing context for this result.
        /// </summary>
        public object? Context { get; }

        /// <summary>
        /// Initialises a new instance of <see cref="Result"/> indicating whether the result succeeded, and the 
        /// exception that occurred in the case where the operation failed.
        /// </summary>
        /// <param name="success">Indicates whether the operation succeeded.</param>
        /// <param name="exception">The exception that occurred in the case where the operation failed.</param>
        /// <param name="context">An object providing context for this result.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if the supplied argument values are invalid. I.e., either:
        /// <list type="bullet">
        /// <item><paramref name="success"/> is true and <paramref name="exception"/> is not null, or</item>
        /// <item><paramref name="success"/> is false and <paramref name="exception"/> is null.</item>
        /// </list>
        /// </exception>
        protected Result(bool success, Exception? exception, object? context)
        {
            switch (success)
            {
                case true when exception != null:
                    throw new ArgumentException("The exception must be null if the result is successful.");
                case false when exception == null:
                    throw new ArgumentException("The exception must not be null if the result is not successful.");
            }

            Success = success;
            Exception = exception;
            Context = context;
        }

        /// <summary>
        /// Creates a successful result for an operation that does not produce a value.
        /// </summary>
        /// <param name="context">An object providing context for this result.</param>
        /// <returns>
        /// A successful <see cref="Result"/> instance.
        /// </returns>
        public static Result Succeeded(object? context = null) => 
            new(true, null, context);

        /// <summary>
        /// Creates a successful result for an operation that produces a value.
        /// </summary>
        /// <typeparam name="T">The type of the result value.</typeparam>
        /// <param name="value">The result value.</param>
        /// <param name="context">An object providing context for this result.</param>
        /// <returns>
        /// A successful <see cref="Result{T}"/> instance containing the specified value.
        /// </returns>
        public static Result<T> Succeeded<T>(T value, object? context = null) =>
            new(true, null, context, value);

        /// <summary>
        /// Creates a failed result for an operation that does not produce a value.
        /// </summary>
        /// <param name="exception">The exception that occurred.</param>
        /// <param name="context">An object providing context for this result.</param>
        /// <returns>
        /// A failed <see cref="Result"/> instance.
        /// </returns>
        public static Result Failed(Exception exception, object? context = null) =>
            new(false, exception, context);

        /// <summary>
        /// Creates a failed result for an operation that produces value.
        /// </summary>
        /// <typeparam name="T">The type of the result value.</typeparam>
        /// <param name="exception">The exception that occurred.</param>
        /// <param name="context">An object providing context for this result.</param>
        /// <returns>
        /// A failed <see cref="Result{T}"/> instance.
        /// </returns>
        public static Result<T> Failed<T>(Exception exception, object? context = null) =>
            new(false, exception, context, default);
    }

    /// <summary>
    /// The result of an operation that produces a value if successful.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    public class Result<T> : Result
    {
        private readonly T? _value;

        /// <summary>
        /// The result value.
        /// </summary>
        /// <exception cref="InvalidOperationException">The result value is not accessible when the result is not successful.</exception>
        public T? Value
        {
            get
            {
                if (!Success)
                    throw new InvalidOperationException(
                        "The result value is not accessible when the result is not successful.");

                return _value;
            }
        }

        /// <summary>
        /// Initialises a new instance of <see cref="Result"/> indicating whether the result succeeded, the
        /// exception that occurred in the case where the operation failed, or alternatively, the value of the
        /// result in the case where the operation succeeded.
        /// </summary>
        /// <param name="success">Indicates whether the operation succeeded.</param>
        /// <param name="exception">The exception that occurred in the case where the operation failed.</param>
        /// <param name="context">An object providing context for this result.</param>
        /// <param name="value">The value of the result in the case where the operation succeeded.</param>
        /// <remarks>
        /// The supplied <paramref name="value"/> is ignored if the operation failed.
        /// </remarks>
        protected internal Result(bool success, Exception? exception, object? context, T? value)
            : base(success, exception, context)
        {
            if (Success) _value = value;
        }
    }
}