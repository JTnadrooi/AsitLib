using AsitLib.Diagnostics;

namespace AsitLib.CommandLine
{
    public sealed class HelpGlobalOption : GlobalOption
    {
        public HelpGlobalOption() : base(OptionInfo.FromType(typeof(bool), ["help", "h"]), "Displays command help.")
        {

        }

        protected override void PreCommand(CommandContext context, object? optionValue)
        {
            if ((bool)optionValue!)
            {
                context.Flags |= ExecutingContextFlags.FullStop;

                context.AddFunction(() => context.Engine.Execute("help " + context.Call.Command.Id).ToOutputString());
            }
        }
    }
}
