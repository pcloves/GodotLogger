using Microsoft.Extensions.Logging;

namespace GodotLogger;

/// <summary>
///     Zero-cost <see cref="ILoggerFactory" /> proxy returned by <see cref="GodotLog.DeferredFactory" />.
///     <see cref="CreateLogger(string)" /> forwards to <see cref="GodotLog.CreateLogger(string)" />,
///     which returns a <see cref="DeferredLogger" /> that only materializes
///     <see cref="GodotLog.Factory" /> on first use.
/// </summary>
/// <remarks>
///     The proxy itself owns no state and loads no assemblies, so it is safe to assign it from
///     constructors, static initializers, or anywhere else that can run inside a Godot C# assembly
///     reload window.
/// </remarks>
internal sealed class DeferredLoggerFactory : ILoggerFactory
{
    /// <summary>
    ///     Shared singleton instance; the proxy is stateless, so a single instance is enough.
    /// </summary>
    internal static DeferredLoggerFactory Instance { get; } = new();

    private DeferredLoggerFactory()
    {
    }

    /// <summary>
    ///     Creates a deferred <see cref="ILogger" /> for the specified category. The underlying
    ///     <see cref="GodotLog.Factory" /> is not materialized until the first log call.
    /// </summary>
    /// <param name="categoryName">The category name for the logger.</param>
    /// <returns>An <see cref="ILogger" /> instance.</returns>
    public ILogger CreateLogger(string categoryName) => GodotLog.CreateLogger(categoryName);

    /// <summary>
    ///     No-op. Providers are configured through <see cref="GodotLog.Configure" /> /
    ///     <c>AddGodotLogger</c>; forwarding an added provider to the real factory would
    ///     materialize it eagerly and defeat the purpose of this proxy.
    /// </summary>
    /// <param name="provider">Ignored.</param>
    public void AddProvider(ILoggerProvider provider)
    {
    }

    /// <summary>
    ///     No-op. This proxy owns no resources and must not dispose the shared
    ///     <see cref="GodotLog.Factory" />.
    /// </summary>
    public void Dispose()
    {
    }
}
