using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib.Diagnostics
{
    public interface ILoggingStyle
    {
        public string GetIndentation(int level, bool hasPrefix = false);
        public string GetHeaderIndentation();
    }

    public sealed class DefaultLoggingStyle : ILoggingStyle
    {
        public string GetIndentation(int level, bool hasPrefix = false)
        {
            if (level <= 0) return hasPrefix ? "^" : "^---";

            StringBuilder builder = new StringBuilder(level * 4 + (hasPrefix ? 1 : 4))
                .Append(' ', level * 4)
                .Append('^');
            if (!hasPrefix) builder.Append("---");

            return builder.ToString();
        }

        public string GetHeaderIndentation() => "^";
    }

    public static class LoggingStyle
    {
        public static ILoggingStyle Default { get; } = new DefaultLoggingStyle();
    }
}
