using Microsoft.Extensions.Logging;
using GodotLogger.Extensions;
using JetBrains.Annotations;

namespace GodotLogger;

/// <summary>
///     Static entry point for quickly creating Godot loggers without explicit DI setup.
///     Configuration is auto-discovered (environment variable, executable directory, project root).
/// </summary>
/// <remarks>
///     <para><b>Configuration lifecycle</b></para>
///     <para>
///     <see cref="Configure" /> may only be called <b>before</b> the first log is written
///     (i.e. before <see cref="Factory" /> is materialized). Calling it later throws
///     <see cref="InvalidOperationException" />.
///     </para>
///     <para>
///     Fields assigned in the <see cref="Configure" /> delegate are effectively "locked":
///     because the delegate is re-executed by the Options pipeline after every JSON reload,
///     it always overwrites values coming from <c>appsettings.json</c>. Fields the delegate
///     does not touch continue to track JSON hot-reload changes.
///     </para>
///     <para>
///     The <see cref="Configure" /> delegate must be idempotent — it may be invoked multiple
///     times due to configuration reloads. Avoid relying on captured mutable state.
///     </para>
/// </remarks>
[PublicAPI]
public static class GodotLog
{
    private static readonly object ConfigureLock = new();
    private static Action<GodotLoggerConfiguration>? _configure;

    private static readonly Lazy<ILoggerFactory> LazyFactory = new(() => LoggerFactory.Create(builder =>
    {
        if (_configure != null)
            builder.AddGodotLogger(_configure);
        else
            builder.AddGodotLogger();
    }));

    /// <summary>
    ///     Configures the Godot logger with the specified delegate. Must be called before the
    ///     first log entry is written; otherwise an <see cref="InvalidOperationException" /> is thrown.
    /// </summary>
    /// <param name="configure">A delegate to configure <see cref="GodotLoggerConfiguration" />.</param>
    /// <exception cref="ArgumentNullException"><paramref name="configure" /> is <see langword="null" />.</exception>
    /// <exception cref="InvalidOperationException">
    ///     The logger factory has already been materialized (i.e. a log was already written or
    ///     <see cref="Factory" /> was already accessed).
    /// </exception>
    public static void Configure(Action<GodotLoggerConfiguration> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        lock (ConfigureLock)
        {
            if (LazyFactory.IsValueCreated)
                throw new InvalidOperationException(
                    "GodotLog.Configure must be called before any log is written. " +
                    "Move the call to your game's entry point (e.g. Main._EnterTree).");

            _configure = configure;
        }
    }

    /// <summary>
    ///     Gets the global <see cref="ILoggerFactory" /> instance pre-configured with the Godot logger provider.
    /// </summary>
    public static ILoggerFactory Factory
    {
        get
        {
            lock (ConfigureLock)
            {
                return LazyFactory.Value;
            }
        }
    }

    /// <summary>
    ///     Creates an <see cref="ILogger{T}" /> for the specified type. The returned instance is a
    ///     lightweight proxy: the underlying <see cref="Factory" /> is not materialized until the
    ///     first call to <see cref="ILogger.Log{TState}" /> or <see cref="ILogger.IsEnabled" />.
    ///     This makes it safe to use in <c>static readonly</c> fields without preventing later
    ///     <see cref="Configure" /> calls.
    /// </summary>
    /// <typeparam name="T">The type to create the logger for.</typeparam>
    /// <returns>An <see cref="ILogger{T}" /> instance.</returns>
    public static ILogger<T> CreateLogger<T>() => new DeferredLogger<T>(static () => Factory);

    /// <summary>
    ///     Creates an <see cref="ILogger" /> for the specified category name. The returned instance
    ///     is a lightweight proxy that defers materializing <see cref="Factory" /> until the first
    ///     log call.
    /// </summary>
    /// <param name="category">The category name for the logger.</param>
    /// <returns>An <see cref="ILogger" /> instance.</returns>
    public static ILogger CreateLogger(string category) => new DeferredLogger(category, static () => Factory);
}