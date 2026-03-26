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

        protected override void PreCommand(CommandContext context, object? optionValue)
        {
            if ((bool)optionValue!)
            {
                _initialVerboseState = _logger.Silent;
                _logger.Silent = false;
            }
        }

        protected override void PostCommand(CommandContext context, object? optionValue)
        {
            if ((bool)optionValue!)
            {
                _logger.Silent = _initialVerboseState;
            }
        }
    }
}
