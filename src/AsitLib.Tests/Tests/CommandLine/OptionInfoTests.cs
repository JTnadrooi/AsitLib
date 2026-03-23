using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib.Tests
{
    file enum TestEnum
    {
        Value0 = 0,
        Value1 = 1,
        Value2 = 2,
        [Signature("three")]
        Value3 = 3,
    }

    [TestClass]
    public class OptionInfoTests
    {
        [TestMethod]
        public void Conform_EnumValueInt_GetsEnumEntryWithValue()
        {
            OptionInfo.FromType(typeof(TestEnum)).Conform("2").Should().Be(TestEnum.Value2);
        }

        [TestMethod]
        public void Conform_EnumValueString_GetsEnumEntryWithValue()
        {
            OptionInfo.FromType(typeof(TestEnum)).Conform("value-2").Should().Be(TestEnum.Value2);
        }

        [TestMethod]
        public void Conform_CustomSignatureEnumValueString_GetsCustomNameEnumEntryWithValue()
        {
            OptionInfo.FromType(typeof(TestEnum)).Conform("three").Should().Be(TestEnum.Value3);
        }

        [TestMethod]
        public void Conform_MultipleIntTokens_ParsesToIntArray()
        {
            ((int[])OptionInfo.FromType(typeof(int[])).Conform(new[] { "4", "2" })!).Should().Equal(new int[] { 4, 2 });
        }

        [TestMethod]
        public void Conform_MultipleStringTokens_ParsesToStringArray()
        {
            ((string[])OptionInfo.FromType(typeof(string[])).Conform(new[] { "e", "a" })!).Should().Equal(new string[] { "e", "a" });
        }

        [TestMethod]
        public void GetInheritedPassingPolicies_EngineWithInheringPositionalPassingPolicies_OverwritesOptionPassingPolicies()
        {
            CommandEngine engine = new CommandEngine() { PassingPolicies = OptionPassingPolicies.Positional };

            CommandInfo info = new DummyCommandInfo("testc", options: new[] {
                OptionInfo.FromType(typeof(string), passingPolicies: OptionPassingPolicies.Named),
            });

            info.GetOptions().All(o => o.GetInheritedPassingPolicies(engine, info) == OptionPassingPolicies.Positional).Should().BeTrue();
        }

        [TestMethod]
        [DataRow(typeof(void))]
        [DataRow(typeof(DBNull))]
        public void Ctor_InvalidType_ThrowsEx(Type type)
        {
            Invoking(() => OptionInfo.FromType(type)).Should().ThrowExactly<ArgumentException>();
        }

        [TestMethod]
        [DataRow("---option")]
        [DataRow("--option")]
        [DataRow("-option")]
        [DataRow("-option")]
        [DataRow("opt ion")]
        public void Ctor_InvalidId_ThrowsEx(string id)
        {
            Invoking(() => OptionInfo.FromType(typeof(string), id)).Should().ThrowExactly<ArgumentException>();
        }
    }
}
