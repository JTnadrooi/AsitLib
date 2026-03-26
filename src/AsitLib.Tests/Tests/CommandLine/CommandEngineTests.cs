using System.ComponentModel.DataAnnotations;
using System.Data;
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
            OneCommandCommandProvider = new DummyCommandProvider("test");
            OneCommandCommandProvider.Commands.Add(new DummyCommandInfo("test-command") { Provider = OneCommandCommandProvider }); // add command here so the property is initialized.
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

            Engine.Providers.Should().ContainSingle();

            Engine.Providers.Single().Value.Name.Should().Be(OneCommandCommandProvider.Name);
            Engine.Commands.Should().ContainKey(OneCommandCommandProvider.GetCommands()[0].Id);
        }

        #region PARSE

        [TestMethod]
        public void Parse_NoArguments()
        {
            Engine.AddCommand(new DummyCommandInfo("cmd"));

            CallInfo parsed = Engine.Parse(["cmd"]);

            parsed.Arguments.Should().BeEmpty();
            parsed.Command.Id.Should().Be("cmd");
        }

        [TestMethod]
        public void Parse_ChildCommandNoArgumentsNoParentCommand_CallsChildCommand()
        {
            Engine.AddCommand(new DummyCommandInfo("group cmd"));

            CallInfo parsed = Engine.Parse(["group", "cmd"]);

            parsed.Arguments.Should().BeEmpty();
            parsed.Command.Id.Should().Be("group cmd");
        }

        [TestMethod]
        public void Parse_ChildCommandPostionalArgumentsNoParentCommand_CallsChildCommand()
        {
            Engine.AddCommand(new DummyCommandInfo("group cmd", options: [
                OptionInfo.FromType(typeof(string), "1"),
            ]));

            CallInfo parsed = Engine.Parse(["group", "cmd", "val1"]);

            parsed.Arguments.Should().Equal([new Argument(new ArgumentTarget(0), ["val1"])]);
            parsed.Command.Id.Should().Be("group cmd");
        }

        [TestMethod]
        public void Parse_PositionalArguments()
        {
            Engine.AddCommand(new DummyCommandInfo("cmd", options: [
                OptionInfo.FromType(typeof(string), "1"),
                OptionInfo.FromType(typeof(string), "2"),
            ]));

            CallInfo parsed = Engine.Parse(["cmd", "val1", "val2"]);

            parsed.Arguments.Should().HaveCount(2);

            parsed.Arguments[0].Tokens[0].Should().Be("val1");

            parsed.Arguments.Should().AllSatisfy(a => a.Tokens.Should().ContainSingle());

            parsed.Arguments.Should().AllSatisfy(a => a.Target.Id.Should().BeNull());

            parsed.Command.Id.Should().Be("cmd");
        }

        [TestMethod]
        public void Parse_NamedArguments()
        {
            Engine.AddCommand(new DummyCommandInfo("cmd", options: [
                OptionInfo.FromType(typeof(string), "arg1"),
                OptionInfo.FromType(typeof(string), "arg2"),
            ]));

            CallInfo parsed = Engine.Parse(["cmd", "--arg1", "val1", "--arg2", "val2"]);

            parsed.Arguments.Should().HaveCount(2);

            parsed.Arguments[0].Tokens.Should().HaveCount(1);

            parsed.Arguments[0].Tokens[0].Should().Be("val1");

            parsed.Arguments.Should().AllSatisfy(a => a.Target.Id.Should().NotBeNull());

            parsed.Command.Id.Should().Be("cmd");
        }

        [TestMethod]
        public void Parse_PositionalAndNamedArguments()
        {
            Engine.AddCommand(new DummyCommandInfo("cmd", options: [
                OptionInfo.FromType(typeof(string), "arg1"),
                OptionInfo.FromType(typeof(string), "arg2"),
            ]));

            CallInfo parsed = Engine.Parse(["cmd", "val1", "--arg2", "val2"]);

            parsed.Arguments.Should().HaveCount(2, because: "'val1' at '#0' and 'val2' at '--arg2'");

            parsed.Arguments[0].Tokens.Should().ContainSingle(because: "only the 'val1' gets passed at '#0'");

            parsed.Arguments[0].Tokens.Should().ContainSingle("val1");
            parsed.Arguments[1].Tokens.Should().ContainSingle("val2");

            parsed.Arguments[0].Target.Index.Should().Be(0, because: "it's passed positionally");
            parsed.Arguments[0].Target.Id.Should().BeNull();
            parsed.Arguments[1].Target.Index.Should().BeNull(because: "it's passed named-ly");
            parsed.Arguments[1].Target.Id.Should().NotBeNull(); // same as above.

            parsed.Command.Id.Should().Be("cmd", because: "'val1' a argument, not a childcommand");
        }

        [TestMethod]
        public void Parse_ChildCommandWithPositionalOptionCall_CallsChildCommand()
        {
            Engine.AddCommand(new DummyCommandInfo("cmdg"));

            Engine.AddCommand(new DummyCommandInfo("cmdg cmd", options: [
                OptionInfo.FromType(typeof(string), "arg1"),
            ]));

            CallInfo parsed = Engine.Parse(["cmdg", "cmd", "val1"]);

            parsed.Command.Id.Should().Be("cmdg cmd", because: "no command 'cmdg cmd val1' exists");

            parsed.Arguments.Should().ContainSingle(because: "'cmdg' exists so cmd gets used as command, resulting in 'val1' as only argument");
        }

        [TestMethod]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(3)]
        [DataRow(4)]
        [DataRow(5)]
        public void Parse_FarChildCommandWithPositionalOptionCall_CallsChildCommand(int depth)
        {
            string[] commandParts = Enumerable.Range(1, depth).Select(i => $"cmdg{i}").Concat(["cmd"]).ToArray();
            string commandId = commandParts.ToJoinedString(" ");

            Engine.AddCommand(new DummyCommandInfo(commandParts[0]));

            Engine.AddCommand(new DummyCommandInfo(commandId));

            CallInfo parsed = Engine.Parse(commandParts);

            parsed.Command.Id.Should().Be(commandId);
        }

        [TestMethod]
        public void Parse_ParentCommandWithNamedOptionCall_CallsParentCommand()
        {
            Engine.AddCommand(new DummyCommandInfo("cmdg", options: [
                OptionInfo.FromType(typeof(string), "arg1"),
            ]));

            Engine.AddCommand(new DummyCommandInfo("cmdg cmd"));

            CallInfo parsed = Engine.Parse(["cmdg", "--arg1", "val1"]);

            parsed.Command.Id.Should().Be("cmdg");

            parsed.Arguments.Should().ContainSingle().Which.Target.SanitizedId.Should().Be("arg1");
        }

        [TestMethod]
        public void Parse_ParentCommandWithQuotedOptionCallThatWouldOtherwiseMatchChildCommand_CallsParentCommand()
        {
            Engine.AddCommand(new DummyCommandInfo("cmdg", options: [
                OptionInfo.FromType(typeof(string), "arg1"),
            ]));

            Engine.AddCommand(new DummyCommandInfo("cmdg cmd"));

            CallInfo parsed = Engine.Parse(["cmdg", "\"cmd\""]);

            parsed.Command.Id.Should().Be("cmdg", because: "quotes are always for inputs (quotes are not allowed in commandnames)"); // .
            parsed.Arguments.Should().ContainSingle(a => a == new Argument(new ArgumentTarget(0), new string[] { "\"cmd\"" }));
        }

        [TestMethod]
        public void Parse_ParentCommandWithArgumentThatDoesNotMatchChildCommand_CallsParentCommand()
        {
            Engine.AddCommand(new DummyCommandInfo("cmdg", options: [
                OptionInfo.FromType(typeof(string), "arg1"),
            ]));

            Engine.AddCommand(new DummyCommandInfo("cmdg cmd"));

            CallInfo parsed = Engine.Parse(["cmdg", "notcmd"]);

            parsed.Command.Id.Should().Be("cmdg", because: "'notcmd' is not found as childcommand, so it tries to get used for the `cmdg` parentcommand input instead");
        }

        [TestMethod]
        public void Parse_PositionalArray()
        {
            Engine.AddCommand(new DummyCommandInfo("cmd", options: [
                OptionInfo.FromType(typeof(string[]), "arg1"),
            ]));

            CallInfo parsed = Engine.Parse(["cmd", "a", "b", "c"]);

            parsed.Arguments[0].Tokens.Should().Equal(["a", "b", "c"], because: "array inputs should be parsed as continued input, not as different positional ones");
            parsed.Arguments[0].Target.Index.Should().Be(0);
        }

        [TestMethod]
        public void Parse_PositionalArrayAndNamedTrailingArgument()
        {
            Engine.AddCommand(new DummyCommandInfo("cmd", options: [
                OptionInfo.FromType(typeof(string[]), "arg1"),
                OptionInfo.FromType(typeof(string), "arg2"),
            ]));

            CallInfo parsed = Engine.Parse(["cmd", "a", "b", "c", "--arg2", "hi"]);

            parsed.Arguments[0].Tokens.Should().Equal(["a", "b", "c"], because: "array inputs should be parsed as continued input, not as different positional ones");
            parsed.Arguments[0].Target.Index.Should().Be(0);

            parsed.Arguments[1].Tokens.Should().Equal(["hi"]);
            parsed.Arguments[1].Target.SanitizedId.Should().Be("arg2");
        }

        [TestMethod]
        public void Parse_PositionalArrayAfterPositionalArgument()
        {
            Engine.AddCommand(new DummyCommandInfo("cmd", options: [
                OptionInfo.FromType(typeof(string), "arg1"),
                OptionInfo.FromType(typeof(string[]), "arg2"),
            ]));

            CallInfo parsed = Engine.Parse(["cmd", "hi", "a", "b", "c"]);

            parsed.Arguments[0].Tokens.Should().Equal(["hi"]);
            parsed.Arguments[0].Target.Index.Should().Be(0);

            parsed.Arguments[1].Tokens.Should().Equal(["a", "b", "c"], because: "array inputs should be parsed as continued input, not as different positional ones");
            parsed.Arguments[1].Target.Index.Should().Be(1);
        }

        [TestMethod]
        public void Parse_PositionalArrayAfterNamedArgument()
        {
            Engine.AddCommand(new DummyCommandInfo("cmd", options: [
                OptionInfo.FromType(typeof(string), "arg1"),
                OptionInfo.FromType(typeof(string[]), "arg2"),
            ]));

            CallInfo parsed = Engine.Parse(["cmd", "hi", "--arg2", "a", "b", "c"]);

            parsed.Arguments[0].Target.Index.Should().Be(0);
            parsed.Arguments[0].Tokens.Should().Equal(["hi"]);

            parsed.Arguments[1].Tokens.Should().Equal(["a", "b", "c"]);
            parsed.Arguments[1].Target.SanitizedId.Should().Be("arg2");
        }

        [TestMethod]
        public void Parse_NonExistentCommand_ThrowsEx()
        {
            Invoking(() => Engine.Parse(["cmd"])).Should().ThrowExactly<CommandNotFoundException>();
        }

        [TestMethod]
        public void Parse_NonExistentGroupCommand_ThrowsEx()
        {
            Invoking(() => Engine.Parse(["group cmd"])).Should().ThrowExactly<CommandNotFoundException>();
        }

        //[TestMethod]
        //public void Parse_NonExistentArgument_ThrowsEx()
        //{
        //    Engine.AddCommand(new DummyCommandInfo("cmd", options: [
        //        OptionInfo.FromType(typeof(string), "arg1"),
        //    ]));

        //    Invoking(() => Engine.Parse(["cmd", "--doesntexist", "aaa"])).Should().ThrowExactly<CommandArgumentException>();
        //}

        [TestMethod]
        public void Parse_DuplicateArgument_ThrowsEx()
        {
            Engine.AddCommand(new DummyCommandInfo("cmd", options: [
                OptionInfo.FromType(typeof(string), "arg1"),
            ]));

            Invoking(() => Engine.Parse(["cmd", "--arg1", "aaa", "--arg1", "bbb"])).Should().ThrowExactly<CommandArgumentException>();
        }

        //[TestMethod]
        //public void Parse_MissingArgument_ThrowsEx()
        //{
        //    Engine.AddCommand(new DummyCommandInfo("cmd", options: [
        //        OptionInfo.FromType(typeof(string), "arg1"),
        //    ]));

        //    Invoking(() => Engine.Parse(["cmd"])).Should().ThrowExactly<CommandException>();
        //}

        //[TestMethod]
        //public void Parse_UnparsableArgument_ThrowsEx()
        //{
        //    Engine.AddCommand(new DummyCommandInfo("cmd", options: [
        //        OptionInfo.FromType(typeof(int), "arg1"),
        //    ]));

        //    Invoking(() => Engine.Parse(["cmd", "aeaeae"])).Should().ThrowExactly<CommandArgumentException>();
        //}

        //[TestMethod]
        //public void Parse_UnusedPositionalArgument_ThrowsEx()
        //{
        //    Engine.AddCommand(new DummyCommandInfo("cmd", options: [
        //        OptionInfo.FromType(typeof(string), "arg1"),
        //    ]));

        //    Invoking(() => Engine.Parse(["cmd", "a", "b"])).Should().ThrowExactly<CommandArgumentException>();
        //}

        //[TestMethod]
        //public void Parse_UnusedNamedArgument_ThrowsEx()
        //{
        //    Engine.AddCommand(new DummyCommandInfo("cmd", options: [
        //        OptionInfo.FromType(typeof(string), "arg1"),
        //    ]));

        //    Invoking(() => Engine.Parse(["cmd", "--arg1", "a", "--arg2", "b"])).Should().ThrowExactly<CommandArgumentException>();
        //}

        //[TestMethod]
        //public void Parse_TargetOptionsWithValidationAttributes_ChecksAgainstAttributes()
        //{
        //    Engine.AddCommand(new DummyCommandInfo("cmd", options: [
        //        OptionInfo.FromType(typeof(string), "arg1", validationAttributes: [new MaxLengthAttribute(1)]),
        //    ]));

        //    Invoking(() => Engine.Parse(["cmd", "--arg1", "aaaaa"])).Should().ThrowExactly<CommandArgumentException>();
        //    Invoking(() => Engine.Parse(["cmd", "aaaaa"])).Should().ThrowExactly<CommandArgumentException>();

        //    Invoking(() => Engine.Parse(["cmd", "--arg1", "a"])).Should().NotThrow();
        //    Invoking(() => Engine.Parse(["cmd", "a"])).Should().NotThrow();
        //}

        [TestMethod]
        public void Parse_TargetGroupThatExistButCommandThatDoesnt_ThrowEx()
        {
            Engine.AddCommand(new DummyCommandInfo("group1 group2 group3 cmd"));

            Invoking(() => Engine.Parse(["group1"])).Should().ThrowExactly<CommandNotFoundException>();
            Invoking(() => Engine.Parse(["group1", "aaaa"])).Should().ThrowExactly<CommandNotFoundException>();
        }

        #endregion

        #region ADD_REMOVE

        [TestMethod]
        public void Add_DuplicateCommandIds_ThrowsEx()
        {
            Engine.AddCommand(new DummyCommandInfo("testc"));

            Invoking(() => Engine.AddCommand(new DummyCommandInfo("testc"))).Should().ThrowExactly<ArgumentException>();
        }

        [TestMethod]
        public void AddAndRemove_GroupedCommand_AddsAndRemovesGroup()
        {
            Engine.AddCommand(new DummyCommandInfo("testg print"));
            Engine.Groups.Should().Contain("testg");

            Engine.RemoveCommand("testg print");

            Engine.Groups.Should().NotContain("testg");
            Engine.Commands.Keys.Should().NotContain("testg print");
        }

        [TestMethod]
        public void AddAndRemove_NestedGroupedCommand_AddsAndRemovesGroups()
        {
            Engine.AddCommand(new DummyCommandInfo("group1 group2 print"));
            Engine.Groups.Should().BeEquivalentTo(["group1", "group1 group2"]);

            Engine.RemoveCommand("group1 group2 print");

            Engine.Groups.Should().BeEmpty();
            Engine.Commands.Keys.Should().NotContain("group1 group2 print");
        }

        [TestMethod]
        public void AddAndRemove_CommandWithMultipleGroups_AddsAndRemovesGroups()
        {
            Engine.AddCommand(new DummyCommandInfo(["group1 cmd", "group2 cmd"]));
            Engine.Groups.Should().Contain("group1");
            Engine.Groups.Should().Contain("group2");

            Engine.RemoveCommand("group1 cmd");

            Engine.Groups.Should().NotContain("group1");
            Engine.Groups.Should().NotContain("group2");
            Engine.Commands.Keys.Should().NotContain("group1 cmd");
            Engine.Commands.Keys.Should().NotContain("group2 cmd");
        }

        [TestMethod]
        [DataRow(1)]
        [DataRow(2)]
        public void AddAndRemove_CommandWithMultipleIdsUsingAnyId(int index)
        {
            Engine.AddCommand(new DummyCommandInfo(["cmd1", "cmd2"]));
            Engine.Commands.Keys.Should().Contain("cmd1");
            Engine.Commands.Keys.Should().Contain("cmd2");

            Engine.RemoveCommand("cmd" + index);

            Engine.Commands.Keys.Should().NotContain("cmd1");
            Engine.Commands.Keys.Should().NotContain("cmd2");
        }

        [TestMethod]
        public void Remove_CommandThatDoesNotExist_ThrowsEx()
        {
            Invoking(() => Engine.RemoveCommand("cmd-that-doesnt-exist")).Should().ThrowExactly<KeyNotFoundException>();
        }

        #endregion
    }
}
