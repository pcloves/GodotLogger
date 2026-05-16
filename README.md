# Gamedo.GodotLogger

[![NuGet](https://img.shields.io/nuget/v/Gamedo.GodotLogger)](https://www.nuget.org/packages/Gamedo.GodotLogger)
[![Target Framework](https://img.shields.io/badge/.NET-8.0-5C2D91)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/github/license/pcloves/GodotLogger)](LICENSE)

[![English](https://img.shields.io/badge/lang-English-blue.svg)](README.md)
[![中文](https://img.shields.io/badge/lang-中文-red.svg)](README.zh-CN.md)

A [Microsoft.Extensions.Logging](https://www.nuget.org/packages/Microsoft.Extensions.Logging) provider that routes .NET
structured logs through Godot 4's built-in output system. Every message is rendered with a customizable template,
colored by log level, and dispatched to `GD.PrintRich`, `GD.PushWarning`, or `GD.PushError` automatically.

---

## ✨ Features

- Implements the standard `ILogger` / `ILoggerProvider` interfaces — drop-in for Godot 4 projects using Microsoft.Extensions.Logging
- Customizable **output template** with placeholders: `{timestamp}`, `{level}`, `{category}`, `{message}`, `{color}`,
  `{exception}`, `{newline}`
- Per-log-level **color mapping** using
  Godot's [named colors](https://docs.godotengine.org/en/stable/tutorials/ui/bbcode_in_richtextlabel.html#named-colors)
  or [hexadecimal color codes](https://docs.godotengine.org/en/stable/tutorials/ui/bbcode_in_richtextlabel.html#hexadecimal-color-codes)
- **Hot-reload** support via `IOptionsMonitor` — configuration changes take effect at runtime
- **Category auto-abbreviation & alignment** (log4j2-style `{category:l20}` / `{category:r10}`)
- **Auto-discovered configuration** — environment variable `GODOT_LOGGER_CONFIG`, executable directory, or Godot project
  root (`res://`)
- **Multiple configuration sources**: auto-discovered `appsettings.json` or code delegate
- **Mode-specific minimum log levels** — `DebugMinLogLevel` (default `Debug`) and `ReleaseMinLogLevel` (default
  `Information`); filtered at the `ILogger.IsEnabled` level for zero formatting overhead
- Template parsing is **cached** and render buffers are pre-sized — minimal allocation at runtime
- Targets **.NET 8** with nullable annotations enabled

---

## 📦 Installation

```shell
dotnet add package Gamedo.GodotLogger
```

Or via the NuGet Package Manager:

```shell
Install-Package Gamedo.GodotLogger
```

---

## 🚀 Quick Start

```csharp
using Godot;
using Microsoft.Extensions.Logging;
using GodotLogger;

namespace MyGame;

public partial class Main : Node
{
    private static readonly ILogger Logger = GodotLog.CreateLogger<Main>();

    public override void _Ready()
    {
        Logger.LogInformation("Hello from GodotLogger!");
    }
}
```

`GodotLog.CreateLogger<T>()` creates a logger with the category name set to `typeof(T).FullName`.
If you prefer a custom category name, use `GodotLog.CreateLogger("MyCategory")` instead.

`GodotLog` auto-discovers `appsettings.json` — just drop the file in your project
root, and it is picked up automatically without any manual setup.

By default, the output aligns categories to 16 characters (configurable via `{category:l<N>}` in the template):

![Log Level Colors](https://raw.githubusercontent.com/pcloves/GodotLogger/master/assets/log-colors.png)

> **💡Tip:** When exporting your game, switch to Release mode to disable BBCode and Debugger overhead:
>
> ```csharp
> GodotLog.Configure(config =>
> {
>     if (OS.HasFeature("release"))
>         config.Mode = LoggerMode.Release;
> });
> ```
>
> Debug mode outputs all levels from `Debug` upward; Release mode restricts to `Information` and above.
> Both are configurable via `DebugMinLogLevel` / `ReleaseMinLogLevel`.

> **💡Tip:** Godot has built-in file logging support! Enable it via **Project → Project Settings → debug/file_logging/enable_file_logging** (toggle to On). You can customize the log path via `debug/file_logging/log_path` (e.g., `user://logs/godot.log`). Since GodotLogger uses `GD.Print*` under the hood, your logs will automatically be saved to file — no extra code needed.

---

## 🎬 Demo

<details>
<summary>Click to show demo</summary>

![Godot Editor Demo](https://raw.githubusercontent.com/pcloves/GodotLogger/master/assets/godot-editor-demo.gif)

</details>

---

## ⚙️ Configuration

### Via code delegate

```csharp
GodotLog.Configure(cfg =>
{
    cfg.Mode = LoggerMode.Debug;
    cfg.DebugMinLogLevel = LogLevel.Debug;
    cfg.ReleaseMinLogLevel = LogLevel.Information;
    cfg.DebugOutputTemplate = "[{timestamp:HH:mm:ss}] [{level:u3}] [{category:l32}] {message}";
    cfg.ReleaseOutputTemplate = "[{timestamp:HH:mm:ss}] [{level:u3}] [{category:l32}] {message}";
    cfg.Colors[LogLevel.Information] = nameof(Colors.SlateBlue);
});
```

### Via `appsettings.json` (auto-discovered)

The parameterless `AddGodotLogger()` searches for `appsettings.json` using the following priority (first match wins):

1. **Environment variable** — `GODOT_LOGGER_CONFIG` pointing to an existing JSON file
2. **Executable directory** — `appsettings.json` next to the running assembly
3. **Godot project root** — `res://appsettings.json` (globalized to an absolute path)

If no file is found, the logger uses sensible defaults.

```json
{
  "Logging": {
    "GodotLogger": {
      "Mode": "Debug",
      "DebugMinLogLevel": "Debug",
      "ReleaseMinLogLevel": "Information",
      "DebugOutputTemplate": "[{timestamp:yyyy-MM-dd HH:mm:ss.fff}] [color={color}][{level:u3}][/color] [{category:l28}] {message}",
      "ReleaseOutputTemplate": "[{timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{level:u3}] [{category:l28}] {message}",
      "Colors": {
        "Trace": "White",
        "Debug": "LawnGreen",
        "Information": "Aqua",
        "Warning": "Orange",
        "Error": "Red",
        "Critical": "Red"
      }
    }
  }
}
```

`AddGodotLogger()` reads the `"GodotLogger"` section from `Logging` by convention and enables hot-reload
via file watcher.

### Per-category log levels via `Logging:LogLevel`

GodotLogger has its own minimum log level mechanism — `DebugMinLogLevel` for Debug mode and
`ReleaseMinLogLevel` for Release mode. The standard .NET `Logging:LogLevel:Default` serves as an
**additional filter layer** on top of that.

If `Logging:LogLevel:Default` is **not explicitly configured** and the .NET minimum level is still the
framework default `Information`, GodotLogger opens the framework-level gate so the current mode's min
level (`DebugMinLogLevel` or `ReleaseMinLogLevel`) controls output as the fallback filter. This makes
setting just `DebugMinLogLevel: "Debug"` work as expected without configuring the .NET logging section
separately.

If `Logging:LogLevel:Default` **is explicitly configured**, it acts as an additional restriction — the
effective level is always the **more restrictive** of the two.

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "MyGame": "Debug",
      "MyGame.": "Warning"
    },
    "GodotLogger": {
      "DebugMinLogLevel": "Trace",
      "ReleaseMinLogLevel": "Information"
    }
  }
}
```

Category keys use **prefix matching** (longest prefix wins):

| Category Key | Matches                                                     |
|--------------|-------------------------------------------------------------|
| `Default`    | All categories (catch-all, lowest priority)                 |
| `MyGame`     | `MyGame` itself                                             |
| `MyGame.`    | `MyGame.Core`, `MyGame.Player`, `MyGame.Player.Input`, etc. |

So `"MyGame": "Debug"` makes the `MyGame` logger verbose, while `"MyGame.": "Warning"`
keeps all its sub-categories quiet. No wildcard/glob syntax is needed.

Relationship between the two filtering layers:

<table>
<tr>
<th>Scenario</th>
<th><code>Logging:LogLevel:Default</code></th>
<th>Mode min level</th>
<th>Effective</th>
</tr>
<tr>
<td>Debug mode, no <code>Logging:LogLevel:Default</code> configured</td>
<td>(falls back to <code>DebugMinLogLevel</code>)</td>
<td><code>Debug</code></td>
<td><code>Debug</code></td>
</tr>
<tr>
<td>Debug mode, <code>"Default": "Warning"</code></td>
<td><code>Warning</code></td>
<td><code>Debug</code></td>
<td><code>Warning</code> (additional filter wins)</td>
</tr>
<tr>
<td>Release mode, no <code>Logging:LogLevel:Default</code> configured</td>
<td>(falls back to <code>ReleaseMinLogLevel</code>)</td>
<td><code>Information</code></td>
<td><code>Information</code></td>
</tr>
<tr>
<td>Release mode, <code>"Default": "Debug"</code></td>
<td><code>Debug</code></td>
<td><code>Information</code></td>
<td><code>Information</code> (mode min level wins)</td>
</tr>
</table>

---

## 📝 Output Template

The template is a string that can contain any literal text plus the following placeholders:

| Placeholder          | Description                                                                                               |
|----------------------|-----------------------------------------------------------------------------------------------------------|
| `{timestamp}`        | Current time (`yyyy-MM-dd HH:mm:ss.fff`)                                                                  |
| `{timestamp:format}` | Current time with a custom `DateTime.ToString` format                                                     |
| `{level}`            | Full log level name, e.g. `Information`                                                                   |
| `{level:u3}`         | Uppercase 3-letter code: `INF`, `WRN`, `ERR`                                                              |
| `{level:l3}`         | Lowercase 3-letter code: `inf`, `wrn`, `err`                                                              |
| `{category}`         | Logger category name, as-is                                                                               |
| `{category:l<N>}`    | Left-aligned, max `N` chars; abbreviated via log4j2-style rule if too long, padded with spaces if shorter |
| `{category:r<N>}`    | Right-aligned, max `N` chars; same abbreviation + padding                                                 |
| `{message}`          | The formatted log message                                                                                 |
| `{exception}`        | The exception's `ToString()` output, or empty                                                             |
| `{color}`            | The Godot color name for the current log level                                                            |
| `{newline}`          | `Environment.NewLine`                                                                                     |

> **Note:**
>
> The `[color=...]` / `[/color]` tags are Godot's BBCode markup used by `GD.PrintRich`. You must surround `{color}`
> with these tags to get colored output. The `{color}` placeholder is only effective in Debug mode; Release mode
> uses `GD.Print` which does not support BBCode.

---

## 🎯 Logger Mode

GodotLogger supports two modes controlled by the `LoggerMode` enum:

| Mode              | Output                          | Exception handling       | Debugger                                     | Default Min Level |
|-------------------|---------------------------------|--------------------------|----------------------------------------------|-------------------|
| `Debug` (default) | `GD.PrintRich` (colored BBCode) | `GD.PrintErr` separately | Warning+ → `GD.PushWarning` / `GD.PushError` | `Debug`           |
| `Release`         | `GD.Print` (plain text)         | `GD.PrintErr` separately | None                                         | `Information`     |

---

## 🎨 Log Level Mapping

| Log Level     | Debug output                      | Release output | Default Color | Enabled by Default |
|---------------|-----------------------------------|----------------|---------------|--------------------|
| `Trace`       | `GD.PrintRich`                    | `GD.Print`     | Gray          | Debug only         |
| `Debug`       | `GD.PrintRich`                    | `GD.Print`     | LawnGreen     | Debug only         |
| `Information` | `GD.PrintRich`                    | `GD.Print`     | Aqua          | Both               |
| `Warning`     | `GD.PrintRich` + `GD.PushWarning` | `GD.Print`     | Orange        | Both               |
| `Error`       | `GD.PrintRich` + `GD.PushError`   | `GD.Print`     | Red           | Both               |
| `Critical`    | `GD.PrintRich` + `GD.PushError`   | `GD.Print`     | DeepPink      | Both               |

In any mode, if the log entry carries an exception, it is additionally printed via `GD.PrintErr`.

You can override any color via the `Colors` dictionary in configuration.
You can adjust these defaults via `DebugMinLogLevel` and `ReleaseMinLogLevel`.

---

## 📁 Project Structure

```
src/
├── GodotLog.cs                     # Static API — auto-discovers configuration, no manual setup needed
├── GodotLogger.cs                  # ILogger implementation
├── GodotLoggerConfiguration.cs     # Options class with mode, colors, templates, min log levels
├── GodotLoggerProvider.cs          # ILoggerProvider (singleton, hot-reload aware)
├── LoggerMode.cs                   # LoggerMode enum (Debug / Release)
├── LogTemplate.cs                  # Template parser + renderer with caching
├── DeferredLogger.cs               # Lazy logger proxy (defers factory creation)
└── Extensions/
    └── LoggingBuilderExtensions.cs # AddGodotLogger() extension methods
```

---

## 📄 License

This project is licensed under the [MIT License](LICENSE).
