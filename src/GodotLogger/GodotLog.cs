using Microsoft.Extensions.Logging;
using GodotLogger.Extensions;
using JetBrains.Annotations;

namespace GodotLogger;

/// <summary>
///     Static entry point for quickly creating Godot loggers without explicit DI setup.
///     Configuration is auto-discovered (environment variable, executable directory, project root).
/// </summary>
/// <remarks>
///     <para><b>Lazy factory</b></para>
///     <para>
///     <see cref="Factory" /> returns a zero-cost <see cref="ILoggerFactory" /> proxy; the real
///     logging pipeline is created lazily on first actual use (the first log write or
///     <see cref="ILogger.IsEnabled" /> call), never by merely reading <see cref="Factory" />.
///     Assigning <see cref="Factory" /> from constructors, static initializers, or anywhere else
///     that can run inside a Godot C# assembly reload window is therefore safe.
///     </para>
///     <para><b>Configuration lifecycle</b></para>
///     <para>
///     <see cref="Configure" /> may only be called <b>before</b> the first log is written
///     (i.e. before the real pipeline is created). Calling it later throws
///     <see cref="InvalidOperationException" />.
///     </para>
///     <para>
///     Fields assigned in the <see cref="Configure" /> delegate are effectively "locked":
///     because the delegate is re-executed by the Options pipeline after every JSON reload, it
///     always overwrites values coming from <c>appsettings.json</c>. Fields the delegate does not
///     touch continue to track JSON hot-reload changes.
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

    // The single public entry point: a zero-cost proxy. Reading it never creates the real pipeline.
    private static readonly DeferredLoggerFactory FactoryProxy = DeferredLoggerFactory.Instance;

    // The real factory, created on first internal need; Lazy<T> guarantees thread-safe creation.
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
    ///     The real logging pipeline has already been created (i.e. a log was already written).
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
    ///     Gets the global <see cref="ILoggerFactory" />. The returned instance is a lightweight
    ///     lazy proxy — merely reading this property never creates the real logging pipeline, so it
    ///     is safe to assign it anywhere (constructors, static initializers, or a Godot C#
    ///     assembly reload window). The real pipeline is created on the first log write or
    ///     <see cref="ILogger.IsEnabled" /> call.
    /// </summary>
    public static ILoggerFactory Factory => FactoryProxy;

    /// <summary>
    ///     Creates an <see cref="ILogger{T}" /> for the specified type. The returned instance is a
    ///     lightweight proxy: the real logging pipeline is not created until the first call to
    ///     <see cref="ILogger.Log{TState}" /> or <see cref="ILogger.IsEnabled" />.
    ///     This makes it safe to use in <c>static readonly</c> fields without preventing later
    ///     <see cref="Configure" /> calls.
    /// </summary>
    /// <typeparam name="T">The type to create the logger for.</typeparam>
    /// <returns>An <see cref="ILogger{T}" /> instance.</returns>
    public static ILogger<T> CreateLogger<T>() => new DeferredLogger<T>(static () => FactoryProxy.Inner);

    /// <summary>
    ///     Creates an <see cref="ILogger" /> for the specified category name. The returned instance
    ///     is a lightweight proxy that defers creating the real logging pipeline until the first
    ///     log call.
    /// </summary>
    /// <param name="category">The category name for the logger.</param>
    /// <returns>An <see cref="ILogger" /> instance.</returns>
    public static ILogger CreateLogger(string category) => new DeferredLogger(category, static () => FactoryProxy.Inner);

    /// <summary>
    ///     Gets the real factory. Internal entry point used by <see cref="DeferredLoggerFactory" />;
    ///     accessing it creates the pipeline if it does not exist yet. Creation is serialized with
    ///     <see cref="Configure" /> so that a concurrent <see cref="Configure" /> call cannot be
    ///     lost to a creation race.
    /// </summary>
    internal static ILoggerFactory RealFactory
    {
        get
        {
            lock (ConfigureLock)
            {
                return LazyFactory.Value;
            }
        }
    }
}
