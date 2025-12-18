using AsitLib.Diagnostics;

namespace AsitLib.CommandLine
{
    public static class LoggerExtensions
    {
        public static VerboseGlobalOption GetVerboseGlobalOption(this Logger logger) => new VerboseGlobalOption(logger);
    }
}
