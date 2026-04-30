using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace GodotLogger;

/// <summary>
///     Configuration options for <see cref="GodotLogger" />, including mode, color mapping, and output templates.
/// </summary>
public class GodotLoggerConfiguration
{
    private static Dictionary<LogLevel, string> DefaultLogLevelColorMap { get; } = new()
    {
        [LogLevel.Trace] = nameof(Godot.Colors.Gray),
        [LogLevel.Debug] = nameof(Godot.Colors.LawnGreen),
        [LogLevel.Information] = nameof(Godot.Colors.Aqua),
        [LogLevel.Warning] = nameof(Godot.Colors.Orange),
        [LogLevel.Error] = nameof(Godot.Colors.Red),
        [LogLevel.Critical] = nameof(Godot.Colors.DeepPink),
    };

    /// <summary>
    ///     Gets or sets the logging mode. <see cref="LoggerMode.Debug" /> uses <c>GD.PrintRich</c> with
    ///     optional <c>GD.PushWarning</c>/<c>GD.PushError</c> for the Debugger panel.
    ///     <see cref="LoggerMode.Release" /> uses <c>GD.Print</c>.
    /// </summary>
    [JsonInclude]
    public LoggerMode Mode { get; set; } = LoggerMode.Debug;

    /// <summary>
    ///     Gets or sets the mapping of log levels to Godot color names used in BBCode output.
    /// </summary>
    [JsonInclude]
    public Dictionary<LogLevel, string> Colors { get; set; } = new(DefaultLogLevelColorMap);

    /// <summary>
    ///     Gets or sets the output template for <see cref="LoggerMode.Debug" />.
    ///     Supports BBCode <c>{color}</c> tags via <c>GD.PrintRich</c>.
    /// </summary>
    [JsonInclude]
    public string DebugOutputTemplate { get; set; } =
        "[{timestamp:yyyy-MM-dd HH:mm:ss.fff}] [color={color}][{level:u3}][/color] [{category:l16}] {message}";

    /// <summary>
    ///     Gets or sets the output template for <see cref="LoggerMode.Release" />.
    ///     No BBCode color tags. Exceptions are printed separately via <c>GD.PrintErr</c>.
    /// </summary>
    [JsonInclude]
    public string ReleaseOutputTemplate { get; set; } =
        "[{timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{level:u3}] [{category:l16}] {message}";

    /// <summary>
    ///     Returns the Godot color name associated with the specified log level.
    /// </summary>
    /// <param name="logLevel">The log level to look up.</param>
    /// <returns>The color name string, or the default color for fallback.</returns>
    public string GetColor(LogLevel logLevel) =>
        Colors.TryGetValue(logLevel, out var color) ? color : DefaultLogLevelColorMap[logLevel];
}