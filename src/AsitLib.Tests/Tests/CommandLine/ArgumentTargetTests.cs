using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib.Tests
{
    [TestClass]
    public class ArgumentTargetTests
    {
        [TestMethod]
        public void Id_ReturnsTokenWithoutLeadingDashes()
        {
            new ArgumentTarget("--token").Id.Should().Be("token");
        }

        [TestMethod]
        public void Id_PositionalTarget_ReturnsNull()
        {
            new ArgumentTarget(0).Id.Should().BeNull();
        }

        [TestMethod]
        public void Token_PositionalTarget_ReturnsNull()
        {
            new ArgumentTarget(0).Token.Should().BeNull();
        }

        [TestMethod]
        public void Index_NamedTarget_ReturnsNull()
        {
            new ArgumentTarget("--token").Index.Should().BeNull();
        }

        [TestMethod]
        [DataRow("--token")]
        [DataRow(2)]
        public void IsMatchFor_MatchingOption_ReturnsTrue(object targetInput)
        {
            ArgumentTarget target = targetInput is int index ? new ArgumentTarget(index) : new ArgumentTarget((string)targetInput);

            target.IsMatchFor(OptionInfo.FromType(typeof(string), "token"), 2).Should().BeTrue();
        }

        [TestMethod]
        [DataRow("--not-token")]
        [DataRow(1)]
        public void IsMatchFor_NotMatchingOption_ReturnsFalse(object targetInput)
        {
            ArgumentTarget target = targetInput is int index ? new ArgumentTarget(index) : new ArgumentTarget((string)targetInput);

            target.IsMatchFor(OptionInfo.FromType(typeof(string), "token"), 2).Should().BeFalse();
        }

        [TestMethod]
        public void IsMatchFor_MatchingOptionForAntiTarget_ReturnsTrue()
        {
            ArgumentTarget target = new ArgumentTarget("--no-token");

            target.IsMatchFor(OptionInfo.FromType(typeof(bool), "token"), -1).Should().BeTrue();
        }

        [TestMethod]
        [DataRow(typeof(string))]
        [DataRow(typeof(int))]
        [DataRow(typeof(Dictionary<string, string>))]
        public void IsMatchFor_MatchingOptionForAntiTargetWithUnAntiableType_ReturnsFalse(Type type)
        {
            ArgumentTarget target = new ArgumentTarget("--no-token");

            target.IsMatchFor(OptionInfo.FromType(type, "token"), -1).Should().BeFalse();
        }

        [TestMethod]
        [DataRow(-1)]
        [DataRow(-100)]
        public void Ctor_InvalidIndex_ThrowsEx(int invalidIndex)
        {
            Invoking(() => new ArgumentTarget(invalidIndex)).Should().ThrowExactly<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        [DataRow("token")]
        [DataRow("---token")]
        [DataRow("--")]
        [DataRow("-")]
        [DataRow("t")]
        [DataRow("--t")]
        [DataRow("-token")]
        public void Ctor_InvalidToken_ThrowsEx(string invalidToken)
        {
            Invoking(() => new ArgumentTarget(invalidToken)).Should().ThrowExactly<ArgumentException>();
        }

        [TestMethod]
        [DataRow("-t")]
        [DataRow("--token")]
        [DataRow("--token-token")]
        public void Ctor_ValidToken(string invalidToken)
        {
            Invoking(() => new ArgumentTarget(invalidToken)).Should().NotThrow();
        }
    }
}
