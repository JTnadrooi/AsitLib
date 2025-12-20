using AsitLib.Diagnostics;

namespace AsitLib.CommandLine
{
    public static class LoggerExtensions
    {
        public static VerboseGlobalOption GetVerboseGlobalOption(this RichLogger logger) => new VerboseGlobalOption(logger);
    }
}
