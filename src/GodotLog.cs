using Microsoft.Extensions.Logging;
using GodotLogger.Extensions;
using JetBrains.Annotations;

namespace GodotLogger;

/// <summary>
///     Static entry point for quickly creating Godot loggers without explicit DI setup.
///     Configuration is auto-discovered (environment variable, executable directory, project root).
/// </summary>
[PublicAPI]
public static class GodotLog
{
    private static Action<GodotLoggerConfiguration>? _configure;

    private static readonly Lazy<ILoggerFactory> LazyFactory = new(() => LoggerFactory.Create(builder =>
    {
        if (_configure != null)
            builder.AddGodotLogger(_configure);
        else
            builder.AddGodotLogger();
    }));

    /// <summary>
    ///     Configures the Godot logger with the specified delegate.
    ///     Must be called before the first access to <see cref="Factory" /> or <see cref="CreateLogger{T}" />.
    /// </summary>
    /// <param name="configure">A delegate to configure <see cref="GodotLoggerConfiguration" />.</param>
    public static void Configure(Action<GodotLoggerConfiguration> configure)
    {
        _configure = configure;
    }

    /// <summary>
    ///     Gets the global <see cref="ILoggerFactory" /> instance pre-configured with the Godot logger provider.
    /// </summary>
    public static ILoggerFactory Factory => LazyFactory.Value;

    /// <summary>
    ///     Creates an <see cref="ILogger{T}" /> for the specified type.
    /// </summary>
    /// <typeparam name="T">The type to create the logger for.</typeparam>
    /// <returns>An <see cref="ILogger{T}" /> instance.</returns>
    public static ILogger<T> CreateLogger<T>() => Factory.CreateLogger<T>();

    /// <summary>
    ///     Creates an <see cref="ILogger" /> for the specified category name.
    /// </summary>
    /// <param name="category">The category name for the logger.</param>
    /// <returns>An <see cref="ILogger" /> instance.</returns>
    public static ILogger CreateLogger(string category) => Factory.CreateLogger(category);
}