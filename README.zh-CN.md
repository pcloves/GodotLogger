# Gamedo.GodotLogger

[![NuGet](https://img.shields.io/nuget/v/Gamedo.GodotLogger)](https://www.nuget.org/packages/Gamedo.GodotLogger)
[![Target Framework](https://img.shields.io/badge/.NET-8.0-5C2D91)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/github/license/pcloves/GodotLogger)](LICENSE)

[![English](https://img.shields.io/badge/lang-English-blue.svg)](README.md)
[![中文](https://img.shields.io/badge/lang-中文-red.svg)](README.zh-CN.md)

一个 [Microsoft.Extensions.Logging](https://www.nuget.org/packages/Microsoft.Extensions.Logging) 提供程序，将 .NET
结构化日志路由到 Godot 4 的内置输出系统。每条消息都使用可自定义的模板渲染，按日志级别着色，并自动分派到
`GD.PrintRich`、`GD.PushWarning` 或 `GD.PushError`。

---

## ✨ 特性

- 实现标准 `ILogger` / `ILoggerProvider` 接口 — 在使用了 Microsoft.Extensions.Logging 的 Godot 4 项目中即插即用
- 可自定义的**输出模板**，支持占位符：`{timestamp}`、`{level}`、`{category}`、`{message}`、`{color}`、
  `{exception}`、`{newline}`
- 按日志级别的**颜色映射**，支持 Godot
  [命名颜色](https://docs.godotengine.org/en/stable/tutorials/ui/bbcode_in_richtextlabel.html#named-colors)
  或[十六进制颜色代码](https://docs.godotengine.org/en/stable/tutorials/ui/bbcode_in_richtextlabel.html#hexadecimal-color-codes)
- **热重载**支持（通过 `IOptionsMonitor`）— 配置更改在运行时即时生效
- **类别自动缩写与对齐**（log4j2 风格的 `{category:l20}` / `{category:r10}`）
- **自动发现配置** — 环境变量 `GODOT_LOGGER_CONFIG`、可执行文件目录或 Godot 项目根目录（`res://`）
- **多种配置来源**：自动发现的 `appsettings.json` 或代码委托
- **按模式设置最低日志级别** — `DebugMinLogLevel`（默认 `Debug`）和 `ReleaseMinLogLevel`（默认
  `Information`）；在 `ILogger.IsEnabled` 级别过滤，零格式化开销
- 模板解析**已缓存**，渲染缓冲区预分配 — 运行时分配极少
- 目标 **.NET 8**，启用可空注解

---

## 📦 安装

```shell
dotnet add package Gamedo.GodotLogger
```

或通过 NuGet 包管理器：

```shell
Install-Package Gamedo.GodotLogger
```

---

## 🚀 快速开始

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

`GodotLog.CreateLogger<T>()` 创建的日志记录器，其类别名称为 `typeof(T).FullName`。
如果你想要自定义类别名称，可以使用 `GodotLog.CreateLogger("MyCategory")`。

`GodotLog` 会自动发现 `appsettings.json` — 只需将文件放到项目根目录，无需任何手动设置即可自动加载。

默认情况下，输出将类别对齐到 16 个字符（可通过模板中的 `{category:l<N>}` 配置）：

![Log Level Colors](https://raw.githubusercontent.com/pcloves/GodotLogger/master/assets/log-colors.png)

> **💡提示：** 导出行程时切换到 Release 模式可以禁用 BBCode 和 Debugger 开销：
>
> ```csharp
> GodotLog.Configure(config =>
> {
>     if (OS.HasFeature("release"))
>         config.Mode = LoggerMode.Release;
> });
> ```
>
> Debug 模式输出从 `Debug` 及以上的所有级别；Release 模式限制为 `Information` 及以上。
> 两者均可通过 `DebugMinLogLevel` / `ReleaseMinLogLevel` 配置。

---

## 🎬 演示

<details>
<summary>点击展开演示</summary>

![Godot Editor Demo](https://raw.githubusercontent.com/pcloves/GodotLogger/master/assets/godot-editor-demo.gif)

</details>

---

## ⚙️ 配置

### 通过代码委托

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

### 通过 `appsettings.json`（自动发现）

无参的 `AddGodotLogger()` 按以下优先级搜索 `appsettings.json`（首个匹配优先）：

1. **环境变量** — `GODOT_LOGGER_CONFIG` 指向一个已存在的 JSON 文件
2. **可执行文件目录** — 运行程序旁边的 `appsettings.json`
3. **Godot 项目根目录** — `res://appsettings.json`（全局化为绝对路径）

如果未找到文件，记录器使用合理的默认值。

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

`AddGodotLogger()` 按约定从 `Logging` 中读取 `"GodotLogger"` 配置节，并通过文件监视器启用热重载。

### 通过 `Logging:LogLevel` 设置按类别过滤日志级别

标准的 .NET 日志管道通过 `Logging:LogLevel` 配置节提供类别级过滤。
这与 `DebugMinLogLevel` / `ReleaseMinLogLevel` **相互独立** — 实际的最低级别取两者中**更严格的那个**。

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

类别键使用**前缀匹配**（最长前缀优先）：

| 类别键       | 匹配范围                                                     |
|--------------|--------------------------------------------------------------|
| `Default`    | 所有类别（兜底，最低优先级）                                  |
| `MyGame`     | `MyGame` 本身                                                |
| `MyGame.`    | `MyGame.Core`、`MyGame.Player`、`MyGame.Player.Input` 等     |

因此 `"MyGame": "Debug"` 让 `MyGame` 记录器输出详细日志，而 `"MyGame.": "Warning"`
则让它的所有子类别保持安静。无需通配符/glob 语法。

两个过滤层之间的关系：

<table>
<tr>
<th>场景</th>
<th><code>Logging:LogLevel</code> 结果</th>
<th>模式最低级别</th>
<th>实际级别</th>
</tr>
<tr>
<td>Debug 模式，无类别过滤</td>
<td>—</td>
<td><code>Debug</code></td>
<td><code>Debug</code></td>
</tr>
<tr>
<td>Release 模式，<code>"MyGame": "Debug"</code></td>
<td><code>Debug</code></td>
<td><code>Information</code></td>
<td><code>Information</code>（模式优先）</td>
</tr>
<tr>
<td>Debug 模式，<code>"MyGame": "Warning"</code></td>
<td><code>Warning</code></td>
<td><code>Debug</code></td>
<td><code>Warning</code>（类别优先）</td>
</tr>
</table>

---

## 📝 输出模板

模板是一个字符串，可以包含任意文本及以下占位符：

| 占位符               | 说明                                                                                 |
|----------------------|--------------------------------------------------------------------------------------|
| `{timestamp}`        | 当前时间（`yyyy-MM-dd HH:mm:ss.fff`）                                                |
| `{timestamp:format}` | 当前时间，使用自定义的 `DateTime.ToString` 格式                                      |
| `{level}`            | 完整日志级别名称，如 `Information`                                                    |
| `{level:u3}`         | 大写的 3 字母代码：`INF`、`WRN`、`ERR`                                              |
| `{level:l3}`         | 小写的 3 字母代码：`inf`、`wrn`、`err`                                              |
| `{category}`         | 日志记录器类别名称，原样输出                                                          |
| `{category:l<N>`}    | 左对齐，最大 `N` 个字符；过长时按 log4j2 规则缩写，过短时用空格填充                   |
| `{category:r<N>`}    | 右对齐，最大 `N` 个字符；缩写和填充规则同上                                           |
| `{message}`          | 格式化后的日志消息                                                                    |
| `{exception}`        | 异常的 `ToString()` 输出，或空                                                        |
| `{color}`            | 当前日志级别的 Godot 颜色名称                                                        |
| `{newline}`          | `Environment.NewLine`                                                                |

> **注意：**
>
> `[color=...]` / `[/color]` 标签是 Godot 的 BBCode 标记，由 `GD.PrintRich` 使用。你必须用这些标签
> 包围 `{color}` 才能获得彩色输出。`{color}` 占位符仅在 Debug 模式生效；Release 模式使用
> `GD.Print`，不支持 BBCode。

---

## 🎯 日志记录器模式

GodotLogger 支持两种模式，由 `LoggerMode` 枚举控制：

| 模式               | 输出方式                        | 异常处理                | Debugger                                     | 默认最低级别    |
|--------------------|--------------------------------|-------------------------|----------------------------------------------|-----------------|
| `Debug`（默认）    | `GD.PrintRich`（彩色 BBCode）   | 额外 `GD.PrintErr`      | Warning+ → `GD.PushWarning` / `GD.PushError` | `Debug`         |
| `Release`          | `GD.Print`（纯文本）            | 额外 `GD.PrintErr`      | 无                                           | `Information`   |

---

## 🎨 日志级别映射

| 日志级别      | Debug 输出                        | Release 输出 | 默认颜色   | 默认启用          |
|---------------|-----------------------------------|--------------|------------|-------------------|
| `Trace`       | `GD.PrintRich`                    | `GD.Print`   | Gray       | 仅 Debug          |
| `Debug`       | `GD.PrintRich`                    | `GD.Print`   | LawnGreen  | 仅 Debug          |
| `Information` | `GD.PrintRich`                    | `GD.Print`   | Aqua       | 两种模式          |
| `Warning`     | `GD.PrintRich` + `GD.PushWarning` | `GD.Print`   | Orange     | 两种模式          |
| `Error`       | `GD.PrintRich` + `GD.PushError`   | `GD.Print`   | Red        | 两种模式          |
| `Critical`    | `GD.PrintRich` + `GD.PushError`   | `GD.Print`   | DeepPink   | 两种模式          |

在任何模式下，如果日志条目携带异常，还会额外通过 `GD.PrintErr` 输出。

你可以通过配置中的 `Colors` 字典覆盖任何颜色。
你也可以通过 `DebugMinLogLevel` 和 `ReleaseMinLogLevel` 调整这些默认值。

---

## 📁 项目结构

```
src/
├── GodotLog.cs                     # 静态 API — 自动发现配置，无需手动设置
├── GodotLogger.cs                  # ILogger 实现
├── GodotLoggerConfiguration.cs     # 选项类 — 模式、颜色、模板、最低日志级别
├── GodotLoggerProvider.cs          # ILoggerProvider（单例，支持热重载）
├── LoggerMode.cs                   # LoggerMode 枚举（Debug / Release）
├── LogTemplate.cs                  # 模板解析器 + 渲染器（带缓存）
├── DeferredLogger.cs               # 延迟记录器代理（延迟工厂创建）
└── Extensions/
    └── LoggingBuilderExtensions.cs # AddGodotLogger() 扩展方法
```

---

## 📄 许可证

本项目基于 [MIT 许可证](LICENSE) 发布。
