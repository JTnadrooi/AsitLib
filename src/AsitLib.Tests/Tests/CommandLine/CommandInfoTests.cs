using AsitLib.CommandLine;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace AsitLib.Tests
{
    public class DummyCommandInfo : CommandInfo
    {
        public OptionInfo[] Options { get; }

        public DummyCommandInfo(string id, string? description = null, bool isGenericFlag = false, OptionInfo[]? options = null, CommandProvider? provider = null)
            : this([id], description, isGenericFlag, options, provider) { }
        public DummyCommandInfo(string[] ids, string? description = null, bool isGenericFlag = false, OptionInfo[]? options = null, CommandProvider? provider = null)
            : base(ids, description ?? "No desc.", isGenericFlag, provider)
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
        public void GroupedCommand_WithInvalidAliases_ThrowsEx()
        {
            CommandInfo info = new DummyCommandInfo(["debug print", "testg writel"]);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Add_GroupedCommandAfterInvalidMainCommand_ThrowsEx()
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
        public void Add_GroupedCommandBeforeInvalidMainCommand_ThrowsEx()
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

        [TestMethod]
        public void IsMainCommandEligible_CommandWithoutOptions_ReturnsTrue()
        {
            CommandInfo info = new DummyCommandInfo("testg", options: []);
            Assert.IsTrue(info.IsMainCommandEligible());
        }

        [TestMethod]
        public void IsMainCommandEligible_CommandWithoutPositonalOptions_ReturnsTrue()
        {
            CommandInfo info = new DummyCommandInfo("testg", options: [
                new OptionInfo("testop", typeof(string)) {
                    PassingOptions = OptionPassingOptions.Named,
                },
                new OptionInfo("testop2", typeof(string)) {
                    PassingOptions = OptionPassingOptions.Named,
                },
            ]);

            Assert.IsTrue(info.IsMainCommandEligible());
        }

        [TestMethod]
        public void IsMainCommandEligible_CommandWithNamedOptions_ReturnsFalse()
        {
            CommandInfo info = new DummyCommandInfo("testg", options: [
                new OptionInfo("testop", typeof(string)),
                new OptionInfo("testop2", typeof(string)),
            ]);

            Assert.IsFalse(info.IsMainCommandEligible());
        }

        [TestMethod]
        [DataRow("testg  print")]
        [DataRow("!print")]
        [DataRow("=print")]
        [DataRow("print+")]
        [DataRow("print@")]
        [DataRow("print  ")]
        [DataRow("print\n")]
        [DataRow("  print")]
        [DataRow("")]
        [DataRow(" ")]
        [DataRow("      ")]
        [DataRow(" print")]
        [DataRow("-print")]
        [DataRow("testg -print")]
        [DataRow("testg1 testg2 -print")]
        public void Contruct_InvalidCommandInfo_ThrowsEx(string name)
        {
            Assert.ThrowsException<InvalidOperationException>(() =>
                new DummyCommandInfo(name)
            );
        }
    }
}
