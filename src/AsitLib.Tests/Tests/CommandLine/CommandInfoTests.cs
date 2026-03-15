namespace AsitLib.Tests
{
    [TestClass]
    public class CommandInfoTests
    {
        [TestMethod]
        public void GroupedCommand_WithAliases_HasGroup()
        {
            CommandInfo info = new DummyCommandInfo(["testg print", "testg writel"]);
            info.Group.Should().Be("testg");
        }

        [TestMethod]
        public void GroupedCommand_WithInvalidAliases_ThrowsEx()
        {
            Invoking(() => new DummyCommandInfo(["debug print", "testg writel"])).Should().Throw<InvalidOperationException>();
        }
        [TestMethod]
        public void IsMainCommandEligible_CommandWithoutOptions_ReturnsTrue()
        {
            CommandInfo info = new DummyCommandInfo("testc");
            info.IsMainCommandEligible().Should().BeTrue();
        }

        [TestMethod]
        public void IsMainCommandEligible_CommandWithoutPositonalOptions_ReturnsTrue()
        {
            CommandInfo info = new DummyCommandInfo("testc", options: [
                OptionInfo.FromType(typeof(string), "a", OptionPassingPolicies.Named),
                OptionInfo.FromType(typeof(string), "b", OptionPassingPolicies.Named),
            ]);

            info.IsMainCommandEligible().Should().BeTrue();
        }

        [TestMethod]
        public void GetInheritedPassingPolicies_CommandWithInheringPositionalPassingPolicies_OverwritesOptionPassingPolicies()
        {
            CommandInfo info = new DummyCommandInfo("testc", options: [
                OptionInfo.FromType(typeof(string), passingPolicies: OptionPassingPolicies.Named),
            ])
            {
                PassingPolicies = OptionPassingPolicies.Positional
            };

            info.GetOptions().All(o => o.GetInheritedPassingPolicies(null, info) == OptionPassingPolicies.Positional).Should().BeTrue();
        }

        [TestMethod]
        public void IsMainCommandEligible_CommandWithNamedOptions_ReturnsFalse()
        {
            CommandInfo info = new DummyCommandInfo("testc", options: [
                OptionInfo.FromType(typeof(string), "a" ),
                OptionInfo.FromType(typeof(string), "b"),
            ]);

            info.IsMainCommandEligible().Should().BeFalse();
        }

        [TestMethod]
        public void Ctor_DuplicateId_ThrowsEx()
        {
            Invoking(() => new DummyCommandInfo(["test", "test"])).Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
        [DataRow("testg  print")]
        [DataRow("print  ")]
        [DataRow("print\n")]
        [DataRow("  print")]
        [DataRow("")]
        [DataRow(" ")]
        [DataRow("      ")]
        [DataRow(" print")]
        [DataRow("testg -print")]
        [DataRow("testg1 testg2 -print")]
        [DataRow("   ")]
        public void Ctor_InvalidId_ThrowsEx(string name)
        {
            Invoking(() => new DummyCommandInfo(name)).Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(3)]
        [DataRow(4)]
        [DataRow(5)]
        [DataRow(6)]
        [DataRow(7)]
        [DataRow(8)]
        [DataRow(9)]
        public void Ctor_AnySubgroupDepth(int subgroupDepth)
        {
            Invoking(() => new DummyCommandInfo(Enumerable.Repeat("str", subgroupDepth).ToJoinedString(" "))).Should().NotThrow();
        }
    }
}
