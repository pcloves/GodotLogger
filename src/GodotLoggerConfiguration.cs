using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace GodotLogger;

/// <summary>
///     Configuration options for <see cref="GodotLogger" />, including color mapping and output template.
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
    ///     Gets or sets the mapping of log levels to Godot color names used in BBCode output.
    /// </summary>
    [JsonInclude]
    public Dictionary<LogLevel, string> Colors { get; set; } = new(DefaultLogLevelColorMap);

    /// <summary>
    ///     Gets or sets the output template string. Supported placeholders:
    ///     <c>{timestamp}</c>, <c>{level}</c>, <c>{category}</c>, <c>{message}</c>,
    ///     <c>{exception}</c>, <c>{color}</c>, <c>{newline}</c>.
    ///     Placeholders can include format specifiers, e.g. <c>{level:u3}</c>, <c>{category:l16}</c>.
    /// </summary>
    [JsonInclude]
    public string OutputTemplate { get; set; } =
        "[{timestamp:yyyy-MM-dd HH:mm:ss.fff}] [color={color}][{level:u3}][/color] [{category:l16}] {message}";

    /// <summary>
    ///     Returns the Godot color name associated with the specified log level.
    /// </summary>
    /// <param name="logLevel">The log level to look up.</param>
    /// <returns>The color name string, or the default color for fallback.</returns>
    public string GetColor(LogLevel logLevel) =>
        Colors.TryGetValue(logLevel, out var color) ? color : DefaultLogLevelColorMap[logLevel];
}