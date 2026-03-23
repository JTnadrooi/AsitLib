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
        public void Ctor_DuplicateId_ThrowsEx()
        {
            Invoking(() => new DummyCommandInfo(["test", "test"])).Should().ThrowExactly<InvalidOperationException>();
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
            Invoking(() => new DummyCommandInfo(name)).Should().ThrowExactly<ArgumentException>();
        }

        [TestMethod]
        [DataRow("group1 group2 cmd")]
        [DataRow("group cmd")]
        [DataRow("cmd")]
        [DataRow("--globaloption")]
        [DataRow("-weird-but-ok")]
        [DataRow("------sure")]
        [DataRow("[]why[]would[]i[]stop[]them?")]
        [DataRow("(&#&@ThisIsNotRegex3938293")]
        [DataRow("pls|dont+do|this")]
        public void Ctor_ValidId(string name)
        {
            Invoking(() => new DummyCommandInfo(name)).Should().NotThrow();
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

        [TestMethod]
        public void Group_DifferingGroupIds_ReturnsNull()
        {
            CommandInfo info = new DummyCommandInfo(["group1 cmd", "group2 cmd"]);

            info.Group.Should().BeNull();
        }

        [TestMethod]
        public void Group_MixedGroupAndNoGroupIds_ReturnsNull()
        {
            CommandInfo info = new DummyCommandInfo(["cmd", "group2 cmd"]);

            info.Group.Should().BeNull();
        }

        [TestMethod]
        public void Group_AlignedGroupedIds()
        {
            CommandInfo info = new DummyCommandInfo(["group1 cmd1", "group1 cmd2"]);

            info.Group.Should().Be("group1");
        }

        [TestMethod]
        public void Group_NoGroupedIds()
        {
            CommandInfo info = new DummyCommandInfo(["cmd1", "cmd2"]);

            info.Group.Should().BeNull();
        }

        [TestMethod]
        public void Groups_DifferingGroupIds_ReturnsAllGroups()
        {
            CommandInfo info = new DummyCommandInfo(["group1 cmd", "group2 cmd"]);

            info.Groups.Should().BeEquivalentTo(["group1", "group2"]);
        }

        [TestMethod]
        public void Groups_MixedGroupAndNoGroupIds_ReturnsFoundGroupsFromAllIds()
        {
            CommandInfo info = new DummyCommandInfo(["cmd", "group2 cmd"]);

            info.Groups.Should().BeEquivalentTo(["group2"]);
        }

        [TestMethod]
        public void Groups_AlignedGroupedIds_ReturnsNoDuplicates()
        {
            CommandInfo info = new DummyCommandInfo(["group1 cmd1", "group1 cmd2"]);

            info.Groups.Should().BeEquivalentTo(["group1"]);
        }

        [TestMethod]
        public void Groups_NoGroupedIds_ReturnsEmptyArray()
        {
            CommandInfo info = new DummyCommandInfo(["cmd1", "cmd2"]);

            info.Groups.Should().BeEquivalentTo(Array.Empty<string>());
        }
    }
}
