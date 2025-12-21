using System.Diagnostics;
using System.IO;
#nullable enable

namespace AsitLib.Diagnostics
{
    public sealed class RichLogger : IThreadSafeRichLogger
    {
        private readonly Stopwatch?[] _timers;
        private readonly int _maxDepth;
        private static readonly object _consoleLock = new object();

        private int _depth;

        public bool Silent { get; set; }
        public TextWriter Out { get; }
        public bool Freeze { get; set; }
        public bool IsConsole => Out == Console.Out;
        private ILoggingStyle Style { get; }

        public RichLogger(ILoggingStyle? style = null, TextWriter? output = null, string? header = null, int maxDepth = 20, int displaysCapasity = 64, bool silent = false)
        {
            Style = style ?? LoggingStyle.Default;
            Out = TextWriter.Synchronized(output ?? Console.Out);
            Silent = silent;

            _maxDepth = maxDepth;
            _timers = new Stopwatch?[maxDepth];

            if (!string.IsNullOrEmpty(header)) Header(header);
        }

        public void Header(string msg)
        {
            if (Silent || Freeze) return;

            Out.WriteLine(Style.GetHeaderIndentation() + "[" + (msg?.ToUpperInvariant() ?? string.Empty) + "]");
        }

        public void Log(string? msg, ReadOnlySpan<object?> displays = default, ConsoleColor? color = null)
        {
            if (Freeze) return;

            void InternalLog(ReadOnlySpan<object?> displays = default)
            {
                if (msg is null) msg = "_NULL_";
                if (msg == string.Empty) msg = "_EMPTY_";

                string normalizedMsg = NormalizeMessage(msg!, out int delta, out char? prefix);

                if (_depth + delta > _maxDepth) throw new InvalidOperationException("Exceeded max indent depth");
                if (!displays.IsEmpty) normalizedMsg = AppendDisplays(normalizedMsg, displays);

                if (!Silent)
                {
                    Out.WriteLine(Style.GetIndentation(_depth, prefix.HasValue) + normalizedMsg);
                    Out.Flush();
                }

                _depth = Math.Max(_depth + delta, 0);

                if (prefix == 's') BeginTiming();
            }

            if (IsConsole && color.HasValue)
            {
                lock (_consoleLock)
                {
                    ConsoleColor original = Console.ForegroundColor;
                    Console.ForegroundColor = color.Value;
                    try { InternalLog(displays); }
                    finally { Console.ForegroundColor = original; }
                }
            }
            else InternalLog(displays);
        }

        public void LogThreadSafe(string? msg, ReadOnlySpan<object?> displays = default, ConsoleColor? color = null)
        {
            if (Freeze) return;

            if (!string.IsNullOrEmpty(msg))
            {
                NormalizeMessage(msg!, out int delta, out char? prefix);
                if (delta != 0) throw new InvalidOperationException("Cannot change depth when thread-safe logging.");
                switch (prefix)
                {
                    case null: break;
                    default: throw new InvalidOperationException("Invalid prefix for thread-safe logging.");
                }
            }

            Log(msg, displays, color);
        }

        private string BuildStatusMsg(string? msg, string status)
        {
            if (Freeze) return string.Empty;

            Stopwatch? sw = EndTiming();
            StringBuilder content = new StringBuilder().Append("<").Append(status);

            if (msg is not null) content.Append("; ").Append(msg.TrimEnd('.'));
            if (sw is not null) content.Append(": time taken: ").Append(sw.ElapsedMilliseconds).Append("ms.");

            return content.ToString();
        }

        public void Fail(string? msg = null)
            => Log(BuildStatusMsg(msg, "failed."), color: ConsoleColor.Red);

        public void Success(string? msg = null)
            => Log(BuildStatusMsg(msg, "success."));

        public void SuccessIf(bool succes, string? msg = null)
        {
            if (succes) Success();
            else Fail();
        }

        public void Warn(string msg, ReadOnlySpan<object?> displays = default)
            => Log("warning: " + msg, displays, color: ConsoleColor.Yellow);
        public void WarnThreadSafe(string msg, ReadOnlySpan<object?> displays = default)
            => LogThreadSafe("warning: " + msg, displays, color: ConsoleColor.Yellow);

        public void Error(string msg, ReadOnlySpan<object?> displays = default)
            => Log("error: " + msg, displays, color: ConsoleColor.Red);
        public void ErrorThreadSafe(string msg, ReadOnlySpan<object?> displays = default)
            => LogThreadSafe("error: " + msg, displays, color: ConsoleColor.Red);

        private void BeginTiming() => _timers[_depth] = Stopwatch.StartNew();

        private Stopwatch? EndTiming()
        {
            Stopwatch? toret = _timers[_depth];
            _timers[_depth] = null;
            return toret;
        }

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

            StringBuilder sb = new StringBuilder(baseMsg).Append(" [");

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
