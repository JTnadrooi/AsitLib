using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
#nullable enable

namespace AsitLib.Debug
{

    //public class BoxDrawingStyle : IDebugStreamStyle
    //{
    //    public string GetIndentation(int level)
    //    {
    //        if (level <= 0) return string.Empty;
    //        string prefix = string.Concat(Enumerable.Repeat("│   ", level - 1));
    //        return prefix + "├──";
    //    }

    //    public string GetHeaderIndentation() => "╭──";
    //}


    public class DebugStream
    {
        public interface IStyle
        {
            public string GetIndentation(int level, bool hasPrefix = false);
            public string GetHeaderIndentation();
        }

        public class DefaultStyle : DebugStream.IStyle
        {
            public string GetIndentation(int level, bool hasPrefix = false) => new string(' ', level * 4) + "^" + (hasPrefix ? string.Empty : "---");
            public string GetHeaderIndentation() => "^";
        }

        public static IStyle Default = new DefaultStyle();

        private IStyle style;
        private int depth = 0;
        private Stopwatch?[] stopwatches;
        private int maxDepth;

        public bool Silent { get; set; } = false;

        public DebugStream(IStyle? style = null, string? header = null, int maxDepth = 20)
        {
            this.style = style ?? Default;
            if (!string.IsNullOrEmpty(header)) Header(header);
            this.maxDepth = maxDepth;
            stopwatches = new Stopwatch[maxDepth];
        }

        public void Header(string msg)
        {
            Console.WriteLine(style.GetHeaderIndentation() + "[" + (msg ?? string.Empty).ToUpperInvariant() + "]");
        }

        public void Log(string? msg, object?[]? displays = null)
        {
            if (Silent) return;

            int delta = 0;

            msg = msg ?? "_NULL_";
            if (msg.StartsWith("\t")) throw new Exception();
            if (msg.StartsWith("<"))
            {
                delta--;
                msg = msg.Substring(1).TrimStart();
            }
            else if (msg.StartsWith(">"))
            {
                delta++;
                msg = msg.Substring(1).TrimStart();
            }
            if (delta + depth > maxDepth) throw new Exception("exceeded max indent depth");
            // prefix handling.
            char? prefix = (msg[0] == '[' && msg[2] == ']') ? msg[1] : null;
            switch (prefix)
            {
                case 's':
                    stopwatches[depth + 1] = Stopwatch.StartNew();
                    Console.WriteLine(prefix);
                    break;
                default: 
                    break;
            }

            if (delta > 0) msg = msg.TrimEnd('.') + "..";
            if (displays != null)
            {
                string list;
                if (displays.Length > 0)
                {
                    IEnumerable<string> displayTypes = displays.Select(d =>
                    {
                        if (d == null) return "_NULL_";
                        return d is string str ? ("\"" + str + "\"") : d.ToString()!; 
                    });
                    list = string.Join(", ", displayTypes);
                }
                else list = "_EMPTY_ARRAY_";
                msg += " [" + list + "]";
            }
            string indentStr = style.GetIndentation(depth, prefix != null);
            Console.WriteLine(indentStr + msg);
            depth = Math.Max(depth + delta, 0);
        }

        public void Warn(string msg, object[]? displays = null)
        {
            ConsoleColor origColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Log("warning: " + (msg ?? string.Empty), displays);
            Console.ForegroundColor = origColor;
        }

        public void Succes()
        {
            Log("<succes: time taken: " + (stopwatches[depth]?.ElapsedMilliseconds.ToString() ?? "??") + "ms.");
            stopwatches[depth] = null;
        }

        public static IStyle GetStyle(string? styleId = null) =>
            (styleId ?? "Default") switch
            {
                // "BoxDrawing" => new BoxDrawingStyle(),
                "Default" => new DefaultStyle(),
                _ => throw new ArgumentException()
            };
    }
}