using Godot;
using Microsoft.Extensions.Logging;

namespace GodotLogger;

/// <summary>
///     An <see cref="ILogger" /> implementation for Godot with template-driven formatting and color support.
///     <para>
///     <b>Message output</b><br/>
///     The template only formats the log message. Exception text is always printed separately
///     via <c>GD.PrintErr</c>, regardless of mode.
///     </para>
///     <para>
///     <b>Mode behavior</b><br/>
///     <see cref="LoggerMode.Debug" /> — uses <c>GD.PrintRich</c> with
///     <c>GD.PushWarning</c>/<c>GD.PushError</c> for the Debugger panel when severity is
///     <see cref="LogLevel.Warning" /> or higher.<br/>
///     <see cref="LoggerMode.Release" /> — uses <c>GD.Print</c> with no Debugger integration.
///     </para>
/// </summary>
/// <param name="name">The category name for this logger instance.</param>
/// <param name="configProvider">A delegate that returns the current <see cref="GodotLoggerConfiguration" />.</param>
public class GodotLogger(string name, Func<GodotLoggerConfiguration> configProvider) : ILogger
{
    /// <summary>
    ///     Writes a log entry to the Godot console.
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
        var message = formatter(state, exception);
        var isDebug = config.Mode == LoggerMode.Debug;

        var templateConfig = isDebug ? config.DebugOutputTemplate : config.ReleaseOutputTemplate;
        var template = LogTemplate.Parse(templateConfig);
        var renderContext = new RenderContext(name, logLevel, message, string.Empty, config.GetColor(logLevel));
        var rendered = template.Render(renderContext);

        if (isDebug)
            GD.PrintRich(rendered);
        else
            GD.Print(rendered);

        if (exception is not null)
            GD.PrintErr(exception.ToString());

        if (isDebug)
        {
            switch (logLevel)
            {
                case LogLevel.Error or LogLevel.Critical:
                    GD.PushError(rendered);
                    break;
                case LogLevel.Warning:
                    GD.PushWarning(rendered);
                    break;
            }
        }
    }

    /// <summary>
    ///     Checks whether the given <see cref="LogLevel" /> is enabled.
    ///     In <see cref="LoggerMode.Debug" /> mode, the minimum level is <see cref="GodotLoggerConfiguration.DebugMinLogLevel" />.
    ///     In <see cref="LoggerMode.Release" /> mode, the minimum level is <see cref="GodotLoggerConfiguration.ReleaseMinLogLevel" />.
    /// </summary>
    /// <param name="logLevel">The log level to check.</param>
    /// <returns><see langword="true" /> if logging is enabled for the specified level; otherwise, <see langword="false" />.</returns>
    public bool IsEnabled(LogLevel logLevel)
    {
        if (logLevel == LogLevel.None)
            return false;

        var config = configProvider();
        var minLevel = config.Mode == LoggerMode.Debug ? config.DebugMinLogLevel : config.ReleaseMinLogLevel;
        return logLevel >= minLevel;
    }

    /// <summary>
    ///     Begins a logical operation scope. Not currently implemented.
    /// </summary>
    /// <param name="state">The scope state.</param>
    /// <typeparam name="TState">The type of the scope state.</typeparam>
    /// <returns>A disposable scope handle, or <see langword="null" />.</returns>
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
}