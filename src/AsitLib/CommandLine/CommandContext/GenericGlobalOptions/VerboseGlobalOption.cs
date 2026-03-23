using AsitLib.Diagnostics;

namespace AsitLib.CommandLine
{
    public sealed class VerboseGlobalOption : GlobalOption
    {
        private bool _initialVerboseState;

        private ILogger _logger;

        public VerboseGlobalOption(ILogger logger) : base(OptionInfo.FromType(typeof(bool), ["verbose", "v"]), "Enables verbose logging.")
        {
            _logger = logger;
        }

        public override void PreCommand(CommandContext context)
        {
            _initialVerboseState = _logger.Silent;
            _logger.Silent = false;
        }

        public override void PostCommand(CommandContext context)
        {
            _logger.Silent = _initialVerboseState;
        }
    }
}
