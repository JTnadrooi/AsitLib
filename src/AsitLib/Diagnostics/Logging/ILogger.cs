using System.IO;

namespace AsitLib.Diagnostics
{
    public interface ILogger
    {
        bool Freeze { get; set; }
        TextWriter Out { get; }
        bool Silent { get; set; }

        public void Log(string? msg, ReadOnlySpan<object?> displays = default);
    }
}
