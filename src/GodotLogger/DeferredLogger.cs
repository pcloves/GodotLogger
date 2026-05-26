using Microsoft.Extensions.Logging;

namespace GodotLogger;

/// <summary>
///     Proxy <see cref="ILogger" /> that defers resolving the real logger until first use, so that
///     declaring <c>static readonly ILogger Logger = GodotLog.CreateLogger&lt;T&gt;()</c> does not
///     prematurely materialize the global <see cref="ILoggerFactory" />.
/// </summary>
internal sealed class DeferredLogger(string category, Func<ILoggerFactory> factoryProvider) : ILogger
{
    private ILogger? _inner;

    private ILogger Inner => _inner ??= factoryProvider().CreateLogger(category);

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
        => Inner.Log(logLevel, eventId, state, exception, formatter);

    public bool IsEnabled(LogLevel logLevel) => Inner.IsEnabled(logLevel);

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => Inner.BeginScope(state);
}

/// <summary>
///     Generic proxy variant of <see cref="DeferredLogger" /> that implements <see cref="ILogger{T}" />.
/// </summary>
internal sealed class DeferredLogger<T>(Func<ILoggerFactory> factoryProvider) : ILogger<T>
{
    private ILogger<T>? _inner;

    private ILogger<T> Inner => _inner ??= factoryProvider().CreateLogger<T>();

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
        => Inner.Log(logLevel, eventId, state, exception, formatter);

    public bool IsEnabled(LogLevel logLevel) => Inner.IsEnabled(logLevel);

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => Inner.BeginScope(state);
}