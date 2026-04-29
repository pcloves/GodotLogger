# Gamedo.GodotLogger

[![NuGet](https://img.shields.io/nuget/v/Gamedo.GodotLogger)](https://www.nuget.org/packages/Gamedo.GodotLogger)
[![Target Framework](https://img.shields.io/badge/.NET-9.0-5C2D91)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/github/license/gamedo/GodotLogger)](LICENSE)

A [Microsoft.Extensions.Logging](https://www.nuget.org/packages/Microsoft.Extensions.Logging) provider that routes .NET
structured logs through Godot 4's built-in output system. Every message is rendered with a customizable template,
colored by log level, and dispatched to `GD.PrintRich`, `GD.PushWarning`, or `GD.PushError` automatically.

---

## ✨ Features

- Implements the standard `ILogger` / `ILoggerProvider` interfaces — drop-in for any .NET host or DI container
- Customizable **output template** with placeholders: `{timestamp}`, `{level}`, `{category}`, `{message}`, `{color}`,
  `{exception}`, `{newline}`
- Per-log-level **color mapping** using Godot's named colors
- **Hot-reload** support via `IOptionsMonitor` — configuration changes take effect at runtime
- **Category auto-abbreviation & alignment** (log4j2-style `{category:l20}` / `{category:r10}`)
- **Multiple configuration sources**: code delegate, `appsettings.json`, standalone JSON file, or raw `IConfiguration`
- Template parsing is **cached** and render buffers are pre-sized — minimal allocation at runtime
- Targets **.NET 9** with nullable annotations enabled

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

By default the output aligns categories to 16 characters (configurable via `{category:l<N>}` in the template):

<pre>
[2026-04-29 10:30:00.123] [<span style="color:aqua">INF</span>] [MyGame.Main      ] Hello from GodotLogger!
[2026-04-29 10:30:01.456] [<span style="color:orange">WRN</span>] [MyGame.Main      ] Something looks suspicious
[2026-04-29 10:30:02.789] [<span style="color:red">ERR</span>] [MyGame.Main      ] Something went wrong
</pre>



---

## ⚙️ Configuration

### Via code delegate

```csharp
builder.AddGodotLogger(cfg =>
{
    cfg.OutputTemplate = "[{timestamp:HH:mm:ss}] [{level:u3}] [{category:l32}] {message}";
    cfg.Colors[LogLevel.Information] = "DodgerBlue";
});
```

### Via `appsettings.json`

```json
{
  "Logging": {
    "GodotLogger": {
      "OutputTemplate": "[{timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{color}][{level:u3}][/color] [{category:l28}] {message}",
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

```csharp
builder.AddGodotLogger();
```

This works because `AddGodotLogger()` calls `AddConfiguration()` internally, which reads the `"GodotLogger"` section
from `Logging` by convention.

### Via a standalone JSON file

```csharp
builder.AddGodotLogger("path/to/logging.json");
```

The file is watched with `reloadOnChange: true` — edit the file and the logger picks it up immediately.

---

## 📝 Output Template

The template is a string that can contain any literal text plus the following placeholders:

| Placeholder          | Description                                                      |
|----------------------|------------------------------------------------------------------|
| `{timestamp}`        | Current time (`yyyy-MM-dd HH:mm:ss.fff`)                         |
| `{timestamp:format}` | Current time with a custom `DateTime.ToString` format            |
| `{level}`            | Full log level name, e.g. `Information`                          |
| `{level:u3}`         | Uppercase 3-letter code: `INF`, `WRN`, `ERR`                     |
| `{level:l3}`         | Lowercase 3-letter code: `inf`, `wrn`, `err`                     |
| `{category}`         | Logger category name, as-is                                      |
| `{category:l<N>}`    | Left-aligned, max `N` chars; abbreviated via log4j2-style rule if too long, padded with spaces if shorter |
| `{category:r<N>}`    | Right-aligned, max `N` chars; same abbreviation + padding        |
| `{message}`          | The formatted log message                                        |
| `{exception}`        | The exception's `ToString()` output, or empty                    |
| `{color}`            | The Godot color name for the current log level                   |
| `{newline}`          | `Environment.NewLine`                                            |

**Default template:**

```
[{timestamp:yyyy-MM-dd HH:mm:ss.fff}] [color={color}][{level:u3}][/color] [{category:l16}] {message}
```

⚠️ The `[color=...]` / `[/color]` tags are Godot's BBCode markup used by `GD.PrintRich`. You must surround `{color}`
with these tags to get colored output.

---

## 🎨 Log Level Mapping

| Log Level     | Godot API        | Default Color |
|---------------|------------------|---------------|
| `Trace`       | `GD.PrintRich`   | Gray          |
| `Debug`       | `GD.PrintRich`   | LawnGreen     |
| `Information` | `GD.PrintRich`   | Aqua          |
| `Warning`     | `GD.PushWarning` | Orange        |
| `Error`       | `GD.PushError`   | Red           |
| `Critical`    | `GD.PushError`   | DeepPink      |

You can override any color via the `Colors` dictionary in configuration.

---

## 📁 Project Structure

```
src/
├── GodotLog.cs                     # Static API — ready-to-use entry point
├── GodotLogger.cs                  # ILogger implementation
├── GodotLoggerConfiguration.cs     # Options class with colors, template
├── GodotLoggerProvider.cs          # ILoggerProvider (singleton, hot-reload aware)
├── LogTemplate.cs                  # Template parser + renderer with caching
└── Extensions/
    └── LoggingBuilderExtensions.cs # AddGodotLogger() extension methods
```

---

## 📄 License

This project is licensed under the [MIT License](LICENSE).
