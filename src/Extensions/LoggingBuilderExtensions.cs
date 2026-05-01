using Godot;
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
    ///     Registers the Godot logger provider with auto-discovered configuration.
    ///     Discovery priority (first found wins):
    ///     <list type="number">
    ///       <item><term>Environment variable <c>GODOT_LOGGER_CONFIG</c></term></item>
    ///       <item><term>Executable directory</term></item>
    ///       <item><term><c>res://appsettings.json</c> (Godot project root)</term></item>
    ///     </list>
    ///     If no configuration file is found, sensible defaults are used.
    /// </summary>
    /// <param name="builder">The <see cref="ILoggingBuilder" /> to add the provider to.</param>
    /// <returns>The same <paramref name="builder" /> for chaining.</returns>
    public static ILoggingBuilder AddGodotLogger(this ILoggingBuilder builder)
    {
        string? configPath = null;

        // 1. Environment variable
        var envPath = System.Environment.GetEnvironmentVariable("GODOT_LOGGER_CONFIG");
        if (!string.IsNullOrEmpty(envPath) && File.Exists(envPath))
            configPath = envPath;

        // 2. Executable directory
        if (configPath == null)
        {
            var processPath = System.Environment.ProcessPath;
            if (processPath != null)
            {
                var exeDir = Path.GetDirectoryName(processPath);
                if (exeDir != null)
                {
                    var candidate = Path.Combine(exeDir, "appsettings.json");
                    if (File.Exists(candidate))
                        configPath = candidate;
                }
            }
        }

        // 3. Godot project root (res://)
        if (configPath == null)
        {
            try
            {
                var candidate = ProjectSettings.GlobalizePath("res://appsettings.json");
                if (File.Exists(candidate))
                    configPath = candidate;
            }
            catch
            {
                // Godot runtime not yet initialized
            }
        }

        if (configPath != null)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(configPath, optional: false, reloadOnChange: true)
                .Build();

            builder.AddConfiguration(configuration.GetSection("Logging"));
        }
        else
        {
            builder.AddConfiguration();
        }

        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, GodotLoggerProvider>());
        LoggerProviderOptions.RegisterProviderOptions<GodotLoggerConfiguration, GodotLoggerProvider>(builder.Services);

        return builder;
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