using AsitLib.CommandLine;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace AsitLib.Tests
{
    public class DummyCommandInfo : CommandInfo
    {
        public OptionInfo[] Options { get; }

        public DummyCommandInfo(string id, string? description = null, bool isGenericFlag = false, bool isEnabled = true, OptionInfo[]? options = null)
            : this([id], description, isGenericFlag, isEnabled, options) { }
        public DummyCommandInfo(string[] ids, string? description = null, bool isGenericFlag = false, bool isEnabled = true, OptionInfo[]? options = null)
            : base(ids, description ?? "No desc.", isGenericFlag, isEnabled)
        {
            Options = options ?? Array.Empty<OptionInfo>();
        }

        public override OptionInfo[] GetOptions() => Options;
        public override object? Invoke(object?[] parameters) => DBNull.Value;
    }

    [TestClass]
    public class CommandInfoTests
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

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public static CommandEngine Engine { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        [TestMethod]
        public void Add_GroupedCommand_AddsGroup()
        {
            Engine.AddCommand(new DummyCommandInfo("testg print"));
            Assert.IsTrue(Engine.Groups.Contains("testg"), "Group didn't register.");
        }

        [TestMethod]
        public void GroupedCommand_WithAliases_HasGroup()
        {
            CommandInfo info = new DummyCommandInfo(["testg print", "testg writel"]);
            Assert.AreEqual(info.Group, "testg");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GroupedCommand_WithInvalidAliases_ThrowsException()
        {
            CommandInfo info = new DummyCommandInfo(["debug print", "testg writel"]);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Add_GroupedCommandAfterInvalidMainCommand_ThrowsException()
        {
            CommandInfo info = new DummyCommandInfo("testg", options: [
                new OptionInfo("a", typeof(string)),
                ]);
            Engine.AddCommand(info);

            CommandInfo info2 = new DummyCommandInfo("testg print");
            Engine.AddCommand(info2);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Add_GroupedCommandBeforeInvalidMainCommand_ThrowsException()
        {
            CommandInfo info = new DummyCommandInfo("testg print");
            Engine.AddCommand(info);

            CommandInfo info2 = new DummyCommandInfo("testg", options: [
                new OptionInfo("a", typeof(string)),
                ]);
            Engine.AddCommand(info2);
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
    }
}
