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
        public void Groups_NotNested()
        {
            new CommandIdentifier("group cmd").Groups.Should().Equal(["group"]);
        }

        [TestMethod]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(3)]
        [DataRow(4)]
        [DataRow(5)]
        public void Groups_NestedGroup(int depth)
        {
            string[] commandParts = Enumerable.Range(1, depth).Select(i => $"group{i}").Concat(["cmd"]).ToArray();

            string fullCommandId = string.Join(" ", commandParts);

            List<string> expectedGroups = new List<string>();
            for (int i = 1; i <= depth; i++)
            {
                string[] groupParts = commandParts.Take(i).ToArray();
                expectedGroups.Add(string.Join(" ", groupParts));
            }

            new CommandIdentifier(fullCommandId).Groups.Should().Equal(expectedGroups);
        }

        [TestMethod]
        public void Groups_NoGroup_ReturnsNull()
        {
            new CommandIdentifier("cmd").Groups.Should().BeEmpty();
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
        public void Ctor_InvalidSource_ThrowsEx(string invalidSource)
        {
            Invoking(() => new CommandIdentifier(invalidSource)).Should().ThrowExactly<ArgumentException>();
        }
    }
}
