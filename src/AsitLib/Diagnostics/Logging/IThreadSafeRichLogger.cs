namespace AsitLib.Diagnostics
{
    public interface IThreadSafeRichLogger : IRichLogger
    {
        public void ErrorThreadSafe(string msg, ReadOnlySpan<object?> displays = default);
        public void LogThreadSafe(string? msg, ReadOnlySpan<object?> displays = default);
        public void WarnThreadSafe(string msg, ReadOnlySpan<object?> displays = default);
    }
}
