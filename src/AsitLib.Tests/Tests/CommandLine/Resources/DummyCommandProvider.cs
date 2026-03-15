namespace AsitLib.Tests
{
    public class DummyCommandProvider : CommandProvider
    {
        public List<CommandInfo> Commands { get; }

        public DummyCommandProvider(string name, List<CommandInfo>? commands = null) : base(name, null)
        {
            Commands = commands ?? new List<CommandInfo>();
        }
        public override CommandInfo[] GetCommands(ICommandInfoFactory? defaultInfoFactory = null)
        {
            return Commands.ToArray();
        }
    }
}
