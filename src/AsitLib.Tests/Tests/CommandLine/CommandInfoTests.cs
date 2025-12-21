namespace AsitLib.Tests
{
    public class DummyCommandInfo : CommandInfo
    {
        public OptionInfo[] Options { get; }

        public DummyCommandInfo(string id, string? description = null, bool isGenericFlag = false, OptionInfo[]? options = null)
            : this(new[] { id }, description, isGenericFlag, options) { }
        public DummyCommandInfo(string[] ids, string? description = null, bool isGenericFlag = false, OptionInfo[]? options = null)
            : base(ids, description ?? "No desc.", isGenericFlag)
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
            Engine.Groups.Contains("testg").Should().BeTrue();
        }

        [TestMethod]
        public void GroupedCommand_WithAliases_HasGroup()
        {
            CommandInfo info = new DummyCommandInfo(new[] { "testg print", "testg writel" });
            info.Group.Should().Be("testg");
        }

        [TestMethod]
        public void GroupedCommand_WithInvalidAliases_ThrowsEx()
        {
            Invoking(() => new DummyCommandInfo(new[] { "debug print", "testg writel" })).Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
        public void Add_GroupedCommandAfterInvalidMainCommand_ThrowsEx()
        {
            CommandInfo info = new DummyCommandInfo("testg", options: new[] {
                new OptionInfo("a", typeof(string)),
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
                new OptionInfo("a", typeof(string)),
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
        public void IsMainCommandEligible_CommandWithoutOptions_ReturnsTrue()
        {
            CommandInfo info = new DummyCommandInfo("testc", options: Array.Empty<OptionInfo>());
            info.IsMainCommandEligible().Should().BeTrue();
        }

        [TestMethod]
        public void IsMainCommandEligible_CommandWithoutPositonalOptions_ReturnsTrue()
        {
            CommandInfo info = new DummyCommandInfo("testc", options: new[] {
                new OptionInfo("testop", typeof(string)) {
                    PassingPolicies = OptionPassingPolicies.Named,
                },
                new OptionInfo("testop2", typeof(string)) {
                    PassingPolicies = OptionPassingPolicies.Named,
                },
            });

            info.IsMainCommandEligible().Should().BeTrue();
        }

        [TestMethod]
        public void GetInheritedPassingPolicies_CommandWithInheringPositionalPassingPolicies_OverwritesOptionPassingPolicies()
        {
            CommandInfo info = new DummyCommandInfo("testc", options: new[] {
                new OptionInfo("testop", typeof(string)) {
                    PassingPolicies = OptionPassingPolicies.Named,
                },
            })
            {
                PassingPolicies = OptionPassingPolicies.Positional
            };

            info.GetOptions().All(o => o.GetInheritedPassingPolicies(null, info) == OptionPassingPolicies.Positional).Should().BeTrue();
        }

        [TestMethod]
        public void GetInheritedPassingPolicies_EngineWithInheringPositionalPassingPolicies_OverwritesOptionPassingPolicies()
        {
            CommandEngine engine = new CommandEngine(OptionPassingPolicies.Positional);

            CommandInfo info = new DummyCommandInfo("testc", options: new[] {
                new OptionInfo("testop", typeof(string)) {
                    PassingPolicies = OptionPassingPolicies.Named,
                },
            });

            info.GetOptions().All(o => o.GetInheritedPassingPolicies(engine, info) == OptionPassingPolicies.Positional).Should().BeTrue();
        }

        [TestMethod]
        public void IsMainCommandEligible_CommandWithNamedOptions_ReturnsFalse()
        {
            CommandInfo info = new DummyCommandInfo("testc", options: new[] {
                new OptionInfo("testop", typeof(string)),
                new OptionInfo("testop2", typeof(string)),
            });

            info.IsMainCommandEligible().Should().BeFalse();
        }

        [TestMethod]
        public void Contruct_DuplicateId_ThrowsEx()
        {
            Invoking(() => new DummyCommandInfo(new[] { "test", "test" })).Should().Throw<InvalidOperationException>();
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
        [DataRow("a")]
        [DataRow("   ")]
        public void Contruct_InvalidIds_ThrowsEx(string name)
        {
            Invoking(() => new DummyCommandInfo(name)).Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
        [DataRow((string[])["testg", "print"])]
        [DataRow((string[])["testg"])]
        public void GetShorthand_NotFound_ReturnsNull(string[] ids)
        {
            new DummyCommandInfo(ids).GetShortHand().Should().BeNull();
        }

        [TestMethod]
        public void GetShorthand()
        {
            new DummyCommandInfo(new[] { "test", "t" }).GetShortHand().Should().Be("t");
        }
    }
}
