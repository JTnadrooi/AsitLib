using AsitLib.Diagnostics;

namespace AsitLib.CommandLine
{
    public sealed class VerboseGlobalOption : GlobalOption
    {
        private bool _initialVerboseState;

        private RichLogger _logger;

        public VerboseGlobalOption(RichLogger logger) : base("verbose", "Enables verbose logging.", "v")
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
