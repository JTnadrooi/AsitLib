using System;
using System.Collections.Generic;
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
            if (level <= 0) return string.Empty;
            string prefix = string.Concat(Enumerable.Repeat("│   ", level - 1));
            return prefix + "├──";
        }

        public string GetHeaderIndentation() => "╭──";
    }

    public class DefaultStyle : IStyle
    {
        public string GetIndentation(int level)
        {
            if (level <= 0) return string.Empty;
            return string.Concat(Enumerable.Repeat("   ", level - 1)) + "^--";
        }
        public string GetHeaderIndentation() => "^";
    }

    public class DebugStream
    {
        private IStyle style;
        private int indentation = 0;

        public bool Silent { get; set; } = false;

        public DebugStream(string? styleId = null, string? header = null)
        {
            this.style = GetStyle(styleId);
            if (!string.IsNullOrEmpty(header)) Header(header);
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

            // append display info if provided.
            if (displays != null)
            {
                string list;
                if (displays.Length > 0)
                {
                    IEnumerable<string> displayTypes = displays.Select(d =>
                    {
                        if (d == null) return "undefined";
                        return d is string str ? ("\"" + str + "\"") : d.ToString()!; 
                    });
                    list = string.Join(", ", displayTypes);
                }
                else list = "_EMPTY_ARRAY_";
                msg += " [" + list + "]";
            }

            string indentStr = style.GetIndentation(indentation);
            Console.WriteLine(indentStr + msg);
            indentation = Math.Max(indentation + delta, 0);
        }

        public void Error(string msg, object[]? displays = null)
        {
            ConsoleColor origColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Log("WARNING: " + (msg ?? string.Empty), displays);
            Console.ForegroundColor = origColor;
        }

        public static IStyle GetStyle(string? styleId = null) =>
            styleId?.ToLowerInvariant() switch
            {
                "BoxDrawing" => new BoxDrawingStyle(),
                _ => new DefaultStyle()
            };
    }
}