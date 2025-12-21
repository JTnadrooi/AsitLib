namespace AsitLib.Tests
{
    public class DummyCommandProvider : CommandProvider
    {
        public List<CommandInfo> Commands { get; }

        public DummyCommandProvider(string name, List<CommandInfo>? commands = null) : base(name, null)
        {
            Commands = commands ?? new List<CommandInfo>();
        }

        public override CommandInfo[] GetCommands()
        {
            return Commands.ToArray();
        }
    }

    [TestClass]
    public class CommandProviderTests
    {
        [TestMethod]
        [DataRow("name#")]
        [DataRow("name-")]
        [DataRow(" name ")]
        [DataRow(" name")]
        [DataRow("na me")]
        [DataRow("-name")]
        [DataRow("")]
        [DataRow(" ")]
        [DataRow("      ")]
        public void Contruct_WithInvalidName_ThrowsEx(string name)
        {
            Invoking(() => new DummyCommandProvider(name)).Should().Throw<InvalidOperationException>();
        }
    }
}
