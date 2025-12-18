namespace AsitLib.CommandLine
{
    public sealed class HelpGlobalOption : GlobalOption
    {
        public HelpGlobalOption() : base("help", "Displays command help.", "h")
        {

        }

        public override void PreCommand(CommandContext context)
        {
            context
                .AddFlag(ExecutingContextFlags.FullStop)
                .AddFunction(() => context.Engine.Execute("help " + context.ArgumentsInfo.CommandId).ToOutputString());
        }
    }
}
