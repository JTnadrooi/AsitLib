using System.IO;

namespace AsitLib.Diagnostics
{
    public interface IRichLogger : ILogger
    {
        ILoggingStyle Style { get; set; }

        void Error(string msg, ReadOnlySpan<object?> displays = default);
        void Fail(string? msg = null);
        void Success(string? msg = null);
        void Warn(string msg, ReadOnlySpan<object?> displays = default);
    }
}