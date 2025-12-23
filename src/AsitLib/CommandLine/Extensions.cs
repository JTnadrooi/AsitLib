using AsitLib.Diagnostics;

namespace AsitLib.CommandLine
{
    public static class LoggerExtensions
    {
        public static VerboseGlobalOption GetVerboseGlobalOption(this ILogger logger) => new VerboseGlobalOption(logger);
    }
}
