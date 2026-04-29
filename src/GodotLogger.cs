using Godot;
using Microsoft.Extensions.Logging;

namespace GodotLogger;

/// <summary>
///     An <see cref="ILogger" /> implementation that writes log output to the Godot console with
///     template-driven formatting and color support.
/// </summary>
/// <param name="name">The category name for this logger instance.</param>
/// <param name="configProvider">A delegate that returns the current <see cref="GodotLoggerConfiguration" />.</param>
public class GodotLogger(string name, Func<GodotLoggerConfiguration> configProvider)
    : ILogger
{
    /// <summary>
    ///     Writes a log entry to the Godot console using <c>GD.PrintRich</c>,
    ///     <c>GD.PushWarning</c>, or <c>GD.PushError</c> depending on the log level.
    /// </summary>
    /// <param name="logLevel">The severity level of the log entry.</param>
    /// <param name="eventId">The event ID associated with the log entry.</param>
    /// <param name="state">The state object to be formatted.</param>
    /// <param name="exception">The exception associated with the log entry, if any.</param>
    /// <param name="formatter">A delegate that formats <paramref name="state" /> and <paramref name="exception" /> into a string.</param>
    /// <typeparam name="TState">The type of the state object.</typeparam>
    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        var config = configProvider();
        var exceptionString = exception?.ToString() ?? string.Empty;
        var message = formatter(state, exception);

        var template = LogTemplate.Parse(config.OutputTemplate);
        var rendered = template.Render(new RenderContext(
            name, logLevel, message, exceptionString,
            config.GetColor(logLevel)));

        switch (logLevel)
        {
            case LogLevel.Error or LogLevel.Critical:
                GD.PushError(rendered);
                break;
            case LogLevel.Warning:
                GD.PushWarning(rendered);
                break;
            default:
                GD.PrintRich(rendered);
                break;
        }
    }

    /// <summary>
    ///     Checks whether the given <see cref="LogLevel" /> is enabled. All levels except <see cref="LogLevel.None" />
    ///     are enabled.
    /// </summary>
    /// <param name="logLevel">The log level to check.</param>
    /// <returns><see langword="true" /> if logging is enabled for the specified level; otherwise, <see langword="false" />.</returns>
    public bool IsEnabled(LogLevel logLevel) => logLevel is not LogLevel.None;

    /// <summary>
    ///     Begins a logical operation scope. Not currently implemented.
    /// </summary>
    /// <param name="state">The scope state.</param>
    /// <typeparam name="TState">The type of the scope state.</typeparam>
    /// <returns>A disposable scope handle, or <see langword="null" />.</returns>
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default;
}