using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        public TextWriter Out { get; set; }
        public bool IsConsole {  get; set; }
        public bool AutoFlush { get; set; }

        public DebugStream(IStyle? style = null, TextWriter? @out = null, string? header = null, int maxDepth = 20)
        {
            this.style = style ?? DebugStream.Default;
            Out = @out ?? Console.Out;
            IsConsole = @out == null;
            AutoFlush = !IsConsole;
            if (!string.IsNullOrEmpty(header)) Header(header);
            this.maxDepth = maxDepth;
            stopwatches = new Stopwatch[maxDepth];
        }

        public void Header(string msg)
        {
            Out.WriteLine(style.GetHeaderIndentation() + "[" + (msg ?? string.Empty).ToUpperInvariant() + "]");
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
            char? prefix = (msg.Length > 3 && msg[0] == '[' && msg[2] == ']') ? msg[1] : null;

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
            Out.WriteLine(indentStr + msg);
            if (AutoFlush) Out.Flush();

            depth = Math.Max(depth + delta, 0);

            switch (prefix)
            {
                case 's':
                    stopwatches[depth] = Stopwatch.StartNew();
                    break;
                default:
                    break;
            }
        }

        private void LogWithColor(ConsoleColor? color, string message, object?[]? displays = null)
        {
            if (IsConsole && color.HasValue)
            {
                var orig = Console.ForegroundColor;
                Console.ForegroundColor = color.Value;
                try { Log(message, displays); }
                finally { Console.ForegroundColor = orig; }
            }
            else
            {
                Log(message, displays);
            }
        }

        private void LogResult(string? msg, bool isSuccess, ConsoleColor? color)
        {
            int idx = depth;
            Stopwatch? sw = stopwatches[idx];
            string defaultMsg = isSuccess ? "success" : "failed";
            string message = sw != null ? $"<{(msg ?? defaultMsg).TrimEnd('.')}: time taken: {sw.ElapsedMilliseconds}ms." : $"<{defaultMsg}";
            LogWithColor(color, message);
            stopwatches[idx] = null;
        }
        public void Fail(string? msg = null) => LogResult(msg, isSuccess: false, color: ConsoleColor.Red);
        public void Success(string? msg = null) => LogResult(msg, isSuccess: true, color: null);
        public void Warn(string msg, object?[]? displays = null) => LogWithColor(ConsoleColor.Red, $"warning: {msg ?? string.Empty}", displays);
    }
}