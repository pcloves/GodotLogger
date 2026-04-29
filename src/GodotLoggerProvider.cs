using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GodotLogger;

/// <summary>
///     An <see cref="ILoggerProvider" /> that creates and caches <see cref="GodotLogger" /> instances.
/// </summary>
[ProviderAlias("GodotLogger")]
public sealed class GodotLoggerProvider : ILoggerProvider
{
    private IDisposable? OnChangeToken { get; }
    private GodotLoggerConfiguration Configuration { get; set; }
    private ConcurrentDictionary<string, GodotLogger> Loggers { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     Initializes a new instance of <see cref="GodotLoggerProvider" /> with configuration sourced from
    ///     <see cref="IOptionsMonitor{GodotLoggerConfiguration}" />, supporting hot reload.
    /// </summary>
    /// <param name="config">The options monitor that provides configuration changes.</param>
    public GodotLoggerProvider(IOptionsMonitor<GodotLoggerConfiguration> config)
    {
        Configuration = config.CurrentValue;
        OnChangeToken = config.OnChange(updatedConfig => Configuration = updatedConfig);
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="GodotLoggerProvider" /> with a fixed configuration snapshot.
    /// </summary>
    /// <param name="currentConfig">The static configuration to use.</param>
    public GodotLoggerProvider(GodotLoggerConfiguration currentConfig)
    {
        Configuration = currentConfig;
    }

    /// <summary>
    ///     Creates or retrieves a cached <see cref="GodotLogger" /> for the given category name.
    /// </summary>
    /// <param name="categoryName">The category name for the logger.</param>
    /// <returns>An <see cref="ILogger" /> instance.</returns>
    public ILogger CreateLogger(string categoryName) =>
        Loggers.GetOrAdd(categoryName, name => new GodotLogger(name, GetCurrentConfig));

    private GodotLoggerConfiguration GetCurrentConfig() => Configuration;

    /// <summary>
    ///     Releases all resources used by the provider, including configuration change subscriptions.
    /// </summary>
    public void Dispose()
    {
        Loggers.Clear();
        OnChangeToken?.Dispose();
    }
}