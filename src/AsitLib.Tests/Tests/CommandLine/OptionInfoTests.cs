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
        public void GetValue_EnumValueInt_GetsEnumEntryWithValue()
        {
            OptionInfo.FromType(typeof(TestEnum)).GetValue("2").Should().Be(TestEnum.Value2);
        }

        [TestMethod]
        public void GetValue_EnumValueString_GetsEnumEntryWithValue()
        {
            OptionInfo.FromType(typeof(TestEnum)).GetValue("value-2").Should().Be(TestEnum.Value2);
        }

        [TestMethod]
        public void GetValue_CustomSignatureEnumValueString_GetsCustomNameEnumEntryWithValue()
        {
            OptionInfo.FromType(typeof(TestEnum)).GetValue("three").Should().Be(TestEnum.Value3);
        }

        [TestMethod]
        public void GetValue_MultipleIntTokens_ParsesToIntArray()
        {
            ((int[])OptionInfo.FromType(typeof(int[])).GetValue(new[] { "4", "2" })!).Should().Equal(new int[] { 4, 2 });
        }

        [TestMethod]
        public void GetValue_MultipleStringTokens_ParsesToStringArray()
        {
            ((string[])OptionInfo.FromType(typeof(string[])).GetValue(new[] { "e", "a" })!).Should().Equal(new string[] { "e", "a" });
        }
    }
}
