using System.Collections.Concurrent;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace GodotLogger;

internal sealed partial class LogTemplate
{
    [GeneratedRegex(@"\{(\w+(?::[^}]*)?)\}")]
    private static partial Regex PlaceholderRegex();

    private static readonly ConcurrentDictionary<string, LogTemplate> Cache = new();

    private static readonly Regex PlaceholderPattern = PlaceholderRegex();

    private const int DynamicOverhead = 23 // timestamp "yyyy-MM-dd HH:mm:ss.fff"
                                        + 3 // level (TRC/DBG/INF/WRN/ERR/CRT)
                                        + 19; // color name (longest: MEDIUM_SPRING_GREEN, see: https://docs.godotengine.org/en/stable/tutorials/ui/bbcode_in_richtextlabel.html#named-colors)

    private Action<StringBuilder, RenderContext>[] Segments { get; }
    private int LiteralLength { get; }

    private LogTemplate(Action<StringBuilder, RenderContext>[] segments, int literalLength)
    {
        Segments = segments;
        LiteralLength = literalLength;
    }

    public static LogTemplate Parse(string template) =>
        Cache.GetOrAdd(template, static t =>
        {
            var (segments, literalLength) = ParseCore(t);
            return new LogTemplate(segments, literalLength);
        });

    public string Render(RenderContext ctx)
    {
        var estimatedLen = LiteralLength
                            + ctx.Message.Length
                            + ctx.Category.Length
                            + DynamicOverhead;

        var sb = new StringBuilder(estimatedLen);
        foreach (var segment in Segments)
        {
            segment(sb, ctx);
        }

        return sb.ToString();
    }

    private static (Action<StringBuilder, RenderContext>[] Segments, int LiteralLength) ParseCore(string template)
    {
        var segments = new List<Action<StringBuilder, RenderContext>>();
        var literalLength = 0;
        var lastEnd = 0;

        foreach (Match match in PlaceholderPattern.Matches(template))
        {
            if (match.Index > lastEnd)
            {
                var literal = template[lastEnd..match.Index];
                literalLength += literal.Length;
                segments.Add((sb, _) => sb.Append(literal));
            }

            segments.Add(CreateSegment(match.Groups[1].Value));
            lastEnd = match.Index + match.Length;
        }

        if (lastEnd < template.Length)
        {
            var literal = template[lastEnd..];
            literalLength += literal.Length;
            segments.Add((sb, _) => sb.Append(literal));
        }

        return ([..segments], literalLength);
    }

    private static Action<StringBuilder, RenderContext> CreateSegment(string key) =>
        key switch
        {
            "category" => static (sb, ctx) => sb.Append(ctx.Category),
            "message" => static (sb, ctx) => sb.Append(ctx.Message),
            "color" => static (sb, ctx) => sb.Append(ctx.Color),
            "level" => static (sb, ctx) => sb.Append(ctx.LogLevel.ToString()),
            "timestamp" => static (sb, _) => sb.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")),
            not null when key.StartsWith("timestamp:", StringComparison.Ordinal)
                => (sb, _) => sb.Append(DateTime.Now.ToString(key[10..])),
            not null when key.StartsWith("level:", StringComparison.Ordinal)
                => (sb, ctx) => sb.Append(FormatLogLevel(ctx.LogLevel, key[6..])),
            not null when key.StartsWith("category:", StringComparison.Ordinal)
                => ParseCategorySegment(key[9..]),
            _ => (sb, _) => sb.Append('{').Append(key).Append('}'),
        };

    private static string FormatLogLevel(LogLevel level, string format) =>
        format switch
        {
            "u3" or "U3" => level switch
            {
                LogLevel.Trace => "TRC",
                LogLevel.Debug => "DBG",
                LogLevel.Information => "INF",
                LogLevel.Warning => "WRN",
                LogLevel.Error => "ERR",
                LogLevel.Critical => "CRT",
                _ => "NON",
            },
            "l3" or "L3" => level switch
            {
                LogLevel.Trace => "trc",
                LogLevel.Debug => "dbg",
                LogLevel.Information => "inf",
                LogLevel.Warning => "wrn",
                LogLevel.Error => "err",
                LogLevel.Critical => "crt",
                _ => "non",
            },
            _ => level.ToString(),
        };

    private static Action<StringBuilder, RenderContext> ParseCategorySegment(string format)
    {
        if (format.Length < 2)
            return static (sb, ctx) => sb.Append(ctx.Category);

        var align = format[0];
        if (align is not 'l' and not 'r')
            return static (sb, ctx) => sb.Append(ctx.Category);

        if (!int.TryParse(format[1..], out var maxLength) || maxLength <= 0)
            return static (sb, ctx) => sb.Append(ctx.Category);

        return align == 'l'
            ? (sb, ctx) =>
            {
                var result = AbbreviateCategory(ctx.Category, maxLength);
                sb.Append(result.PadRight(maxLength));
            }
            : (sb, ctx) =>
            {
                var result = AbbreviateCategory(ctx.Category, maxLength);
                sb.Append(result.PadLeft(maxLength));
            };
    }

    private static string AbbreviateCategory(string category, int maxLength)
    {
        if (string.IsNullOrEmpty(category) || maxLength <= 0 || category.Length <= maxLength)
            return category;

        var parts = category.Split('.');

        if (parts.Length == 1)
            return category[..maxLength];

        for (var i = 0; i < parts.Length - 1; i++)
        {
            if (parts[i].Length > 1)
                parts[i] = parts[i][..1];
        }

        var list = parts.ToList();
        while (list.Count > 1)
        {
            var joined = string.Join(".", list);
            if (joined.Length <= maxLength)
                return joined;
            list.RemoveAt(0);
        }

        var last = list[0];
        return last.Length > maxLength ? last[..maxLength] : last;
    }
}

internal readonly record struct RenderContext(
    string Category,
    LogLevel LogLevel,
    string Message,
    string Color);