using System;
using System.Linq;
#nullable enable

namespace AsitLib.Debug
{
    public interface IStyle
    {
        public string GetIndentation(int level);
        public string GetHeaderIndentation();
    }

    public class BoxDrawingStyle : IStyle
    {
        public string GetIndentation(int level)
        {
            if (level <= 0)
                return string.Empty;

            // for each level >1 draw "│   ", then at current level "├──".
            var prefix = string.Concat(Enumerable.Repeat("│   ", level - 1));
            return prefix + "├──";
        }

        public string GetHeaderIndentation()
        {
            return "╭──";
        }
    }

    public class DefaultStyle : IStyle
    {
        public string GetIndentation(int level)
        {
            if (level <= 0)
                return string.Empty;

            var prefix = string.Concat(Enumerable.Repeat("   ", level - 1));
            return prefix + "^--";
        }

        public string GetHeaderIndentation()
        {
            return "^";
        }
    }

    public class DebugStream
    {
        private int indentation = 0;
        public bool Silent { get; set; } = false;
        private readonly IStyle style;

        public DebugStream(string? styleId = null, string? header = null)
        {
            this.style = GetStyle(styleId);
            if (!string.IsNullOrEmpty(header))
                Header(header);
        }

        public void Header(string msg)
        {
            msg = msg ?? string.Empty;
            Console.WriteLine(style.GetHeaderIndentation() + "[" + msg.ToUpperInvariant() + "]");
        }

        public void Log(string msg, object[]? displays = null)
        {
            if (Silent) return;

            msg = msg ?? string.Empty;
            int delta = 0;

            // adjust delta for closing marker '<'.
            if (msg.StartsWith("<"))
            {
                delta--;
                msg = msg.Substring(1).TrimStart();
            }
            // adjust delta for opening marker '>'.
            else if (msg.StartsWith(">"))
            {
                delta++;
                msg = msg.Substring(1).TrimStart();
            }

            // count all leading tabs as additional indent.
            int tabs = msg.TakeWhile(c => c == '\t').Count();
            if (tabs > 0)
            {
                delta += tabs;
                msg = msg.Substring(tabs);
            }

            // trailing dots indicate indent after.
            if (msg.EndsWith("..")) delta++;

            // append display info if provided.
            if (displays != null)
            {
                string list = displays.Length > 0
                    ? string.Join(", ", displays.Select(d =>
                        d == null ? "undefined"
                        : (d is string str ? $"\"{str}\""
                                          : d.GetType().Name)))
                    : "_EMPTY_ARRAY_";
                msg += " [" + list + "]";
            }

            string indentStr = style.GetIndentation(indentation);
            Console.WriteLine(indentStr + msg);
            indentation = Math.Max(indentation + delta, 0);
        }

        public void Error(string msg, object[]? displays = null)
        {
            var origColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Log("WARNING: " + (msg ?? string.Empty), displays);
            Console.ForegroundColor = origColor;
        }

        public static IStyle GetStyle(string? styleId = null)
        {
            return string.Equals(styleId, "box-drawing",
                                 StringComparison.OrdinalIgnoreCase)
                ? (IStyle)new BoxDrawingStyle()
                : new DefaultStyle();
        }
    }
}