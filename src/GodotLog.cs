using Microsoft.Extensions.Logging;
using GodotLogger.Extensions;
using JetBrains.Annotations;

namespace GodotLogger;

/// <summary>
///     Static entry point for quickly creating Godot loggers without explicit DI setup.
/// </summary>
[PublicAPI]
public static class GodotLog
{
    private const string DefaultConfigFile = "appsettings.json";

    private static readonly Lazy<ILoggerFactory> LazyFactory =
        new(() => LoggerFactory.Create(builder => builder.AddGodotLogger(ConfigFilePath ?? DefaultConfigFile)));

    /// <summary>
    ///     Gets or sets the JSON configuration file path. Must be set before the first call to
    ///     <see cref="Factory" /> or <see cref="CreateLogger{T}" />. Default: <c>"appsettings.json"</c>.
    /// </summary>
    public static string ConfigFilePath { get; set; } = DefaultConfigFile;

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