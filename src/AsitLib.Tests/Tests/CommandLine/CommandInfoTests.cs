namespace AsitLib.Tests
{
    [TestClass]
    public class CommandInfoTests
    {
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
        public void IsMainCommandEligible_CommandWithoutOptions_ReturnsTrue()
        {
            CommandInfo info = new DummyCommandInfo("testc", options: Array.Empty<OptionInfo>());
            info.IsMainCommandEligible().Should().BeTrue();
        }

        [TestMethod]
        public void IsMainCommandEligible_CommandWithoutPositonalOptions_ReturnsTrue()
        {
            CommandInfo info = new DummyCommandInfo("testc", options: new[] {
                OptionInfo.FromType(typeof(string), "a", OptionPassingPolicies.Named),
                OptionInfo.FromType(typeof(string), "b", OptionPassingPolicies.Named),
            });

            info.IsMainCommandEligible().Should().BeTrue();
        }

        [TestMethod]
        public void GetInheritedPassingPolicies_CommandWithInheringPositionalPassingPolicies_OverwritesOptionPassingPolicies()
        {
            CommandInfo info = new DummyCommandInfo("testc", options: new[] {
                OptionInfo.FromType(typeof(string), passingPolicies: OptionPassingPolicies.Named),
            })
            {
                PassingPolicies = OptionPassingPolicies.Positional
            };

            info.GetOptions().All(o => o.GetInheritedPassingPolicies(null, info) == OptionPassingPolicies.Positional).Should().BeTrue();
        }

        [TestMethod]
        public void IsMainCommandEligible_CommandWithNamedOptions_ReturnsFalse()
        {
            CommandInfo info = new DummyCommandInfo("testc", options: new[] {
                OptionInfo.FromType(typeof(string), "a" ),
                OptionInfo.FromType(typeof(string), "b"),
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
        public void Contruct_InvalidId_ThrowsEx(string name)
        {
            Invoking(() => new DummyCommandInfo(name)).Should().Throw<InvalidOperationException>();
        }
    }
}
