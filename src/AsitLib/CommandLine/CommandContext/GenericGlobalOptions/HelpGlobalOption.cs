namespace AsitLib.CommandLine
{
    public sealed class HelpGlobalOption : GlobalOption
    {
        public HelpGlobalOption() : base(OptionInfo.FromType(typeof(bool), ["help", "h"]), "Displays command help.")
        {

        }

        public override void PreCommand(CommandContext context)
        {
            context
                .AddFlag(ExecutingContextFlags.FullStop)
                .AddFunction(() => context.Engine.Execute("help " + context.Call.CommandId).ToOutputString());
        }
    }
}
