namespace GodotLogger;

/// <summary>
///     Defines the operating mode of the Godot logger.
/// </summary>
public enum LoggerMode
{
    /// <summary>
    ///     Debug mode — logs are written with the <c>DEBUG</c> define active,
    ///     enabling verbose output during development.
    /// </summary>
    Debug,

    /// <summary>
    ///     Release mode — logs are written in a production-ready configuration,
    ///     typically with optimized formatting and reduced overhead.
    /// </summary>
    Release
}
