using System.IO;

namespace AsitLib.Diagnostics
{
    public interface IRichLogger
    {
        public bool Freeze { get; set; }
        public bool IsConsole { get; }
        public TextWriter Out { get; }
        public bool Silent { get; set; }

        public void Error(string msg, ReadOnlySpan<object?> displays = default);
        public void Fail(string? msg = null);
        public void Header(string msg);
        public void Log(string? msg, ReadOnlySpan<object?> displays = default, ConsoleColor? color = null);
        public void Success(string? msg = null);
        public void SuccessIf(bool succes, string? msg = null)
        {
            if (succes) Success();
            else Fail();
        }
        public void Warn(string msg, ReadOnlySpan<object?> displays = default);
    }

    public interface IThreadSafeRichLogger : IRichLogger
    {
        public void ErrorThreadSafe(string msg, ReadOnlySpan<object?> displays = default);
        public void LogThreadSafe(string? msg, ReadOnlySpan<object?> displays = default, ConsoleColor? color = null);
        public void WarnThreadSafe(string msg, ReadOnlySpan<object?> displays = default);
    }
}