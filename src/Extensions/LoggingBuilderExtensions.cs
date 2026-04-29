using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;

namespace GodotLogger.Extensions;

/// <summary>
///     Extension methods for setting up Godot logging in an <see cref="ILoggingBuilder" />.
/// </summary>
[PublicAPI]
public static class LoggingBuilderExtensions
{
    /// <summary>
    ///     Registers the Godot logger provider with the default configuration.
    /// </summary>
    /// <param name="builder">The <see cref="ILoggingBuilder" /> to add the provider to.</param>
    /// <returns>The same <paramref name="builder" /> for chaining.</returns>
    public static ILoggingBuilder AddGodotLogger(this ILoggingBuilder builder)
    {
        builder.AddConfiguration();

        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, GodotLoggerProvider>());
        LoggerProviderOptions.RegisterProviderOptions<GodotLoggerConfiguration, GodotLoggerProvider>(builder.Services);

        return builder;
    }

    /// <summary>
    ///     Registers the Godot logger provider with configuration from an <see cref="IConfiguration" /> source.
    /// </summary>
    /// <param name="builder">The <see cref="ILoggingBuilder" /> to add the provider to.</param>
    /// <param name="configuration">The configuration to bind to <see cref="GodotLoggerConfiguration" />.</param>
    /// <returns>The same <paramref name="builder" /> for chaining.</returns>
    public static ILoggingBuilder AddGodotLogger(this ILoggingBuilder builder, IConfiguration configuration)
    {
        builder.Services.AddSingleton(configuration);
        return builder.AddGodotLogger();
    }

    /// <summary>
    ///     Registers the Godot logger provider with configuration loaded from a JSON file.
    /// </summary>
    /// <param name="builder">The <see cref="ILoggingBuilder" /> to add the provider to.</param>
    /// <param name="configFilePath">Path to the JSON configuration file.</param>
    /// <returns>The same <paramref name="builder" /> for chaining.</returns>
    public static ILoggingBuilder AddGodotLogger(this ILoggingBuilder builder, string configFilePath)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(configFilePath, optional: true, reloadOnChange: true)
            .Build();

        builder.Services.AddSingleton(configuration);
        return builder.AddGodotLogger();
    }

    /// <summary>
    ///     Registers the Godot logger provider with configuration specified by a delegate.
    /// </summary>
    /// <param name="builder">The <see cref="ILoggingBuilder" /> to add the provider to.</param>
    /// <param name="configure">A delegate to configure <see cref="GodotLoggerConfiguration" />.</param>
    /// <returns>The same <paramref name="builder" /> for chaining.</returns>
    public static ILoggingBuilder AddGodotLogger(this ILoggingBuilder builder,
        Action<GodotLoggerConfiguration> configure)
    {
        builder.AddGodotLogger();
        builder.Services.Configure(configure);

        return builder;
    }
}