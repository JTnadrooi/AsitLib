using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
#nullable enable

namespace AsitLib.Debug
{
    public sealed class DebugStream
    {
        public interface IStyle
        {
            string GetIndentation(int level, bool hasPrefix = false);
            string GetHeaderIndentation();
        }

        public sealed class DefaultStyle : IStyle
        {
            public string GetIndentation(int level, bool hasPrefix = false)
            {
                if (level <= 0) return hasPrefix ? "^" : "^---";
                return new string(' ', level * 4) + "^" + (hasPrefix ? string.Empty : "---");
            }

            public string GetHeaderIndentation() => "^";
        }

        public static IStyle Default { get; } = new DefaultStyle();

        private readonly IStyle _style;
        private readonly Stack<Stopwatch?> _timers;
        private readonly int _maxDepth;

        private int _depth;

        public bool Silent { get; set; }
        public TextWriter Out { get; }
        public bool IsConsole { get; }
        public bool AutoFlush { get; }
        public int DisplaysCapasity { get; }

        public DebugStream(IStyle? style = null, TextWriter? output = null, string? header = null, int maxDepth = 20, int displaysCapasity = 64)
        {
            _style = style ?? Default;
            Out = output ?? Console.Out;
            IsConsole = output is null;
            AutoFlush = !IsConsole;
            DisplaysCapasity = displaysCapasity;

            _maxDepth = maxDepth;
            _timers = new Stack<Stopwatch?>(maxDepth);

            if (!string.IsNullOrEmpty(header)) Header(header);
        }

        public void Header(string msg) => Out.WriteLine(_style.GetHeaderIndentation() + "[" + (msg?.ToUpperInvariant() ?? string.Empty) + "]");

        public void Log(string? msg, ReadOnlySpan<object?> displays = default)
        {
            if (Silent) return;
            if (string.IsNullOrEmpty(msg)) msg = "_NULL_";

            string normalizedMsg = NormalizeMessage(msg!, out int delta, out char? prefix);

            if (_depth + delta > _maxDepth) throw new InvalidOperationException("Exceeded max indent depth");
            if (!displays.IsEmpty) normalizedMsg = AppendDisplays(normalizedMsg, displays);

            Out.WriteLine(_style.GetIndentation(_depth, prefix.HasValue) + normalizedMsg);
            if (AutoFlush) Out.Flush();

            _depth = Math.Max(_depth + delta, 0);

            if (prefix == 's') BeginTiming();
        }
        private string BuildStatusMsg(string? msg, string status)
        {
            Stopwatch? sw = EndTiming();
            StringBuilder content = new StringBuilder().Append("<").Append(status);

            if (msg != null) content.Append("; ").Append(msg.TrimEnd('.'));
            if (sw != null) content.Append(": time taken: ").Append(sw.ElapsedMilliseconds).Append("ms.");

            return content.ToString();
        }

        public void Fail(string? msg = null)
            => LogWithColor(ConsoleColor.Red, BuildStatusMsg(msg, "failed"));

        public void Success(string? msg = null)
            => Log(BuildStatusMsg(msg, "success"));

        public void SuccessIf(bool succes, string? msg = null)
        {
            if (succes) Success();
            else Fail();
        }

        public void Warn(string msg, ReadOnlySpan<object?> displays = default)
            => LogWithColor(ConsoleColor.Red, "warning: " + msg, displays);

        public void LogWithColor(ConsoleColor? color, string message, ReadOnlySpan<object?> displays = default)
        {
            if (IsConsole && color.HasValue)
            {
                ConsoleColor original = Console.ForegroundColor;
                Console.ForegroundColor = color.Value;
                try { Log(message, displays); }
                finally { Console.ForegroundColor = original; }
            }
            else Log(message, displays);
        }

        private void BeginTiming()
        {
            if (_timers.Count < _maxDepth) _timers.Push(Stopwatch.StartNew());
        }

        private Stopwatch? EndTiming() => _timers.Count > 0 ? _timers.Pop() : null;

        private string NormalizeMessage(string msg, out int delta, out char? prefix)
        {
            delta = 0;
            prefix = null;

            if (msg.Length > 0 && msg[0] == '<')
            {
                delta--;
                msg = msg.Substring(1).TrimStart();
            }
            else if (msg.Length > 0 && msg[0] == '>')
            {
                delta++;
                msg = msg.Substring(1).TrimStart();
                if (msg.Length > 0) msg = msg.TrimEnd('.') + "..";
            }

            if (msg.Length > 3 && msg[0] == '[' && msg[2] == ']') prefix = msg[1];

            return msg;
        }

        private string AppendDisplays(string baseMsg, ReadOnlySpan<object?> displays)
        {
            if (displays.Length == 0) return baseMsg + " [_EMPTY_ARRAY_]";

            StringBuilder sb = DisplaysCapasity == -1 ? new StringBuilder() : new StringBuilder(baseMsg.Length + DisplaysCapasity)
                .Append(baseMsg)
                .Append(" [");

            for (int i = 0; i < displays.Length; i++)
            {
                if (i > 0) sb.Append(", ");
                object? d = displays[i];
                if (d is null) sb.Append("_NULL_");
                else if (d is string str) sb.Append('"').Append(str).Append('"');
                else sb.Append(d);
            }

            sb.Append(']');
            return sb.ToString();
        }
    }
}
