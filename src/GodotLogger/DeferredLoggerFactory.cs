using Microsoft.Extensions.Logging;

namespace GodotLogger;

/// <summary>
///     Lazy <see cref="ILoggerFactory" /> proxy returned by <see cref="GodotLog.Factory" />.
/// </summary>
/// <remarks>
///     <para>
///     The proxy itself owns no resources and loads no assemblies, so it is safe to assign it from
///     constructors, static initializers, or anywhere else that can run inside a Godot C# assembly
///     reload window. The real factory (<see cref="GodotLog.RealFactory" />) is created on first
///     actual use; providers queued before that point are registered when it is created.
///     </para>
///     <para>
///     <see cref="CreateLogger(string)" /> returns <see cref="DeferredLogger" /> instances, which
///     resolve the real factory on first use and therefore perform no eager work.
///     </para>
/// </remarks>
internal sealed class DeferredLoggerFactory : ILoggerFactory
{
    /// <summary>
    ///     Shared singleton instance. The proxy must be unique because it owns the queue of
    ///     providers that have not been registered yet.
    /// </summary>
    internal static DeferredLoggerFactory Instance { get; } = new();

    private readonly object _lock = new();
    private List<ILoggerProvider>? _pendingProviders;
    private ILoggerFactory? _real;

    private DeferredLoggerFactory()
    {
    }

    /// <summary>
    ///     Gets the real factory: created on first access, together with the registration of any
    ///     providers queued before creation.
    /// </summary>
    internal ILoggerFactory Inner
    {
        get
        {
            lock (_lock)
            {
                if (_real == null)
                {
                    _real = GodotLog.RealFactory;
                    if (_pendingProviders != null)
                    {
                        foreach (var provider in _pendingProviders)
                            _real.AddProvider(provider);

                        _pendingProviders = null;
                    }
                }

                return _real;
            }
        }
    }

    /// <summary>
    ///     Creates a deferred <see cref="ILogger" /> for the specified category. The real factory
    ///     is resolved on the first log call.
    /// </summary>
    /// <param name="categoryName">The category name for the logger.</param>
    /// <returns>An <see cref="ILogger" /> instance.</returns>
    public ILogger CreateLogger(string categoryName) => GodotLog.CreateLogger(categoryName);

    /// <summary>
    ///     Registers a logging provider. If the real factory does not exist yet, the provider is
    ///     queued and registered when the factory is created; otherwise it is forwarded immediately.
    /// </summary>
    /// <remarks>
    ///     Queueing is slightly more permissive than standard Microsoft.Extensions.Logging
    ///     semantics, where a provider added after a logger was created does not affect that
    ///     logger. Here, providers added during the deferred window take effect once the real
    ///     factory is first created.
    /// </remarks>
    /// <param name="provider">The provider to register.</param>
    public void AddProvider(ILoggerProvider provider)
    {
        lock (_lock)
        {
            if (_real != null)
            {
                _real.AddProvider(provider);
                return;
            }

            (_pendingProviders ??= []).Add(provider);
        }
    }

    /// <summary>
    ///     Disposes the real factory if it has already been created; otherwise does nothing. It
    ///     never creates a factory just to dispose it.
    /// </summary>
    public void Dispose()
    {
        lock (_lock)
        {
            _real?.Dispose();
        }
    }
}
