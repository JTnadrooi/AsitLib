using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib.Tests
{
    [TestClass]
    public class CommandIdentifierTests
    {
        [TestMethod]
        public void Group_NotNested()
        {
            new CommandIdentifier("group cmd").Group.Should().Be("group");
        }

        [TestMethod]
        public void Group_NestedGroup()
        {
            new CommandIdentifier("group1 group2 cmd").Group.Should().Be("group1 group2");
        }

        [TestMethod]
        public void Group_NoGroup_ReturnsNull()
        {
            new CommandIdentifier("cmd").Group.Should().BeNull();
        }

        [TestMethod]
        public void GetGroups_NotNested()
        {
            new CommandIdentifier("group cmd").GetGroups().Should().Equal(["group"]);
        }

        [TestMethod]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(3)]
        [DataRow(4)]
        [DataRow(5)]
        public void GetGroups_NestedGroup(int depth)
        {
            string[] commandParts = Enumerable.Range(1, depth).Select(i => $"group{i}").Concat(["cmd"]).ToArray();

            string fullCommandId = string.Join(" ", commandParts);

            List<string> expectedGroups = new List<string>();
            for (int i = 1; i <= depth; i++)
            {
                string[] groupParts = commandParts.Take(i).ToArray();
                expectedGroups.Add(string.Join(" ", groupParts));
            }

            new CommandIdentifier(fullCommandId).GetGroups().Should().Equal(expectedGroups);
        }

        [TestMethod]
        public void GetGroups_NoGroup_ReturnsNull()
        {
            new CommandIdentifier("cmd").GetGroups().Should().BeEmpty();
        }

        [TestMethod]
        [DataRow("--cmd")]
        [DataRow("-c")]
        [DataRow("--c-m-d")]
        [DataRow("-cmd")] // should be allowed if someone wants to add a shorthand-combination like command.
        [DataRow("-----cmd")] // allowed.
        public void IsGenericFlag_GenericFlagSource_ReturnsTrue(string source)
        {
            new CommandIdentifier(source).IsGenericFlag.Should().BeTrue();
        }

        [TestMethod]
        [DataRow("cmd")]
        [DataRow("c-")]
        [DataRow("cmd--")]
        [DataRow("cmd-")]
        public void IsGenericFlag_NotGenericFlagSource_ReturnsFalse(string source)
        {
            new CommandIdentifier(source).IsGenericFlag.Should().BeFalse();
        }

        [TestMethod]
        [DataRow("group1 group2 cmd")]
        [DataRow("group1 cmd")]
        [DataRow("cmd")]
        [DataRow("-cmd")]
        [DataRow("--cmd")]
        [DataRow("---cmd")]
        public void ToString_ReturnsSource(string source)
        {
            new CommandIdentifier(source).ToString().Should().Be(source);
        }

        [TestMethod]
        [DataRow("group  cmd")]
        [DataRow("cmd  ")]
        [DataRow("cmd\n")]
        [DataRow("  cmd")]
        [DataRow("")]
        [DataRow(" ")]
        [DataRow("      ")]
        [DataRow(" cmd")]
        [DataRow("group -cmd")]
        [DataRow("group1 group2 -cmd")]
        [DataRow("   ")]
        [DataRow("-group cmd")]
        public void Ctor_InvalidSource_ThrowsEx(string invalidSource)
        {
            Invoking(() => new CommandIdentifier(invalidSource)).Should().ThrowExactly<ArgumentException>();
        }
    }
}
