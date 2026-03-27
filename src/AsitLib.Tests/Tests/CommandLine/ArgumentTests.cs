using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib.Tests
{
    [TestClass]
    public class ArgumentTests
    {
        [TestMethod]
        public void CanTargetChildCommand_NamedTarget_ReturnsFalse()
        {
            new Argument(new ArgumentTarget("--token"), ["value"]).CanTargetChildCommand.Should().BeFalse();
        }

        [TestMethod]
        public void CanTargetChildCommand_MultipleTokens_ReturnsFalse()
        {
            new Argument(new ArgumentTarget(0), ["value1", "value2"]).CanTargetChildCommand.Should().BeFalse();
        }

        [TestMethod]
        public void CanTargetChildCommand_ChildCommandTarget_ReturnsTrue()
        {
            new Argument(new ArgumentTarget(0), ["value1"]).CanTargetChildCommand.Should().BeTrue();
        }

        [TestMethod]
        public void Ctor_PositionalNoTokens_ThrowsEx() // this is impossible.
        {
            Invoking(() => new Argument(new ArgumentTarget(0), [])).Should().ThrowExactly<ArgumentException>();
        }
    }
}
