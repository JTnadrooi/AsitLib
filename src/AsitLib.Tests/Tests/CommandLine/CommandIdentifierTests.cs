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
            Invoking(() => new CommandIdentifier(invalidSource)).Should().Throw<InvalidOperationException>();
        }
    }
}
