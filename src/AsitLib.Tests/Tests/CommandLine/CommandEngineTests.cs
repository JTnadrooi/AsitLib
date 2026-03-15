using System.Diagnostics.CodeAnalysis;

namespace AsitLib.Tests.Tests.CommandLine
{
    [TestClass]
    public class CommandEngineTests
    {
        [TestInitialize]
        public void TestInit()
        {
            Engine = new CommandEngine();
        }

        [TestCleanup]
        public void TestCleanup()
        {

        }

        [NotNull]
        public static CommandEngine? Engine { get; private set; }

        public static DummyCommandProvider OneCommandCommandProvider { get; }

        static CommandEngineTests()
        {
            OneCommandCommandProvider = new DummyCommandProvider("test", commands: []);

            OneCommandCommandProvider.Commands.Add(new DummyCommandInfo("test-command") { Provider = OneCommandCommandProvider });

        }

        [TestMethod]
        public void GetProviderCommands_FromTestProvider()
        {
            Engine.AddProvider(new TestCommandProvider());

            Engine.GetProviderCommands("test").Should().AllSatisfy(c => c.Provider!.Name.Should().Be("test"));
        }

        [TestMethod]
        public void AddProvider_AddsProvider()
        {
            Engine.AddProvider(OneCommandCommandProvider);

            Engine.Providers.Should().HaveCount(1);

            Engine.Providers.Single().Value.Name.Should().Be(OneCommandCommandProvider.Name);
            Engine.Commands.Should().ContainKey(OneCommandCommandProvider.GetCommands()[0].Id);
        }

        [TestMethod]
        public void Parse_NoArguments()
        {
            CallInfo parsed = Engine.Parse(["cmd"]);

            parsed.Arguments.Should().BeEmpty();
            parsed.CommandId.Should().Be("cmd");
        }

        [TestMethod]
        public void Parse_PositionalArguments()
        {
            CallInfo parsed = Engine.Parse(["cmd", "val1", "val2"]);

            parsed.Arguments.Should().HaveCount(2);

            parsed.Arguments[0].Tokens[0].Should().Be("val1");

            parsed.Arguments.Should().AllSatisfy(a => a.Tokens.Should().HaveCount(1));

            parsed.Arguments.Should().AllSatisfy(a => a.Target.Id.Should().BeNull());

            parsed.CommandId.Should().Be("cmd");
        }

        [TestMethod]
        public void Parse_NamedArguments()
        {
            CallInfo parsed = Engine.Parse(["cmd", "--arg1", "val1", "val1_2", "--arg2", "val2"]);

            parsed.Arguments.Should().HaveCount(2);

            parsed.Arguments[0].Tokens.Should().HaveCount(2);

            parsed.Arguments[0].Tokens[0].Should().Be("val1");

            parsed.Arguments.Should().AllSatisfy(a => a.Target.Id.Should().NotBeNull());

            parsed.CommandId.Should().Be("cmd");
        }

        [TestMethod]
        public void Parse_PositionalAndNamedArguments()
        {
            CallInfo parsed = Engine.Parse(["cmd", "val1", "--arg2", "val2"]);

            parsed.Arguments.Should().HaveCount(2);

            parsed.Arguments[0].Tokens.Should().HaveCount(1);

            parsed.Arguments[0].Tokens[0].Should().Be("val1");
            parsed.Arguments[1].Tokens[0].Should().Be("val2");

            parsed.Arguments[0].Target.Index.Should().Be(0);
            parsed.Arguments[1].Target.Index.Should().BeNull();

            parsed.Arguments[1].Target.Id.Should().NotBeNull();

            parsed.CommandId.Should().Be("cmd");
        }

        [TestMethod]
        public void Parse_GroupedCommand()
        {
            CommandInfo info2 = new DummyCommandInfo("cmdg");
            Engine.AddCommand(info2);

            CommandInfo info = new DummyCommandInfo("cmdg cmd");
            Engine.AddCommand(info);

            CallInfo parsed = Engine.Parse(["cmdg", "cmd", "val1"]);

            parsed.CommandId.Should().Be("cmdg cmd");

            parsed.Arguments.Should().HaveCount(1);
        }

        [TestMethod]
        public void Parse_GroupCommandWithNamedOptionCall()
        {
            CommandInfo info2 = new DummyCommandInfo("cmdg");
            Engine.AddCommand(info2);

            CommandInfo info = new DummyCommandInfo("cmdg cmd");
            Engine.AddCommand(info);

            CallInfo parsed = Engine.Parse(["cmdg", "--arg1", "val1"]);

            parsed.CommandId.Should().Be("cmdg");

            parsed.Arguments.Should().HaveCount(1);
        }

        [TestMethod]
        public void Add_GroupedCommandAfterInvalidMainCommand_ThrowsEx()
        {
            CommandInfo info = new DummyCommandInfo("testg", options: new[] {
                 OptionInfo.FromType( typeof(string)),
            });

            Engine.AddCommand(info);

            CommandInfo info2 = new DummyCommandInfo("testg print");

            Invoking(() => Engine.AddCommand(info2)).Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
        public void Add_GroupedCommandBeforeInvalidMainCommand_ThrowsEx()
        {
            CommandInfo info = new DummyCommandInfo("testg print");
            Engine.AddCommand(info);

            CommandInfo info2 = new DummyCommandInfo("testg", options: new[] {
                 OptionInfo.FromType(typeof(string)),
            });
            Invoking(() => Engine.AddCommand(info2)).Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
        public void Add_GroupedCommandBeforeMainCommand()
        {
            CommandInfo info = new DummyCommandInfo("testg print");
            Engine.AddCommand(info);

            CommandInfo info2 = new DummyCommandInfo("testg");
            Engine.AddCommand(info2);
        }

        [TestMethod]
        public void Add_GroupedCommandAfterMainCommand()
        {
            CommandInfo info = new DummyCommandInfo("testg");
            Engine.AddCommand(info);

            CommandInfo info2 = new DummyCommandInfo("testg print");
            Engine.AddCommand(info2);
        }

        [TestMethod]
        public void Add_GroupedCommand_AddsGroup()
        {
            Engine.AddCommand(new DummyCommandInfo("testg print"));
            Engine.Groups.Should().Contain("testg");
        }
    }
}
