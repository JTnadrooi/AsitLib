using System.Numerics;

namespace AsitLib.Tests
{
    [TestClass]
    public class MathALTests
    {
        [TestMethod]
        public void Invert_ValidValue()
        {
            MathAL.Invert(5f, 0f).Should().Be(-5f);
        }

        [TestMethod]
        [DataRow(float.NaN)]
        [DataRow(float.PositiveInfinity)]
        [DataRow(float.NegativeInfinity)]
        public void Invert_InvalidValue_ThrowsEx(float invalidValue)
        {
            Invoking(() => MathAL.Invert(invalidValue, 0f)).Should().ThrowExactly<ArgumentException>();
        }

        [TestMethod]
        public void Normalize_Degrees_ValidValue()
        {
            MathAL.Normalize(370f).Should().Be(10f);
        }

        [TestMethod]
        public void Normalize_Radians_ValidValue()
        {
            MathAL.Normalize((float)Math.PI * 2 + 0.5f, true).Should().BeApproximately(0.5f, 0.0001f);
        }

        [TestMethod]
        public void Difference_ValidValues()
        {
            MathAL.Difference(10f, 5f).Should().Be(5f);
        }

        [TestMethod]
        public void Average_ValidValues()
        {
            MathAL.Average(10f, 20f, 30f).Should().Be(20f);
        }

        [TestMethod]
        [DataRow(float.NaN)]
        [DataRow(float.PositiveInfinity)]
        [DataRow(float.NegativeInfinity)]
        public void Average_InvalidValueNaN_ThrowsEx(float invalidValue)
        {
            Invoking(() => MathAL.Average(10f, 20f, invalidValue)).Should().ThrowExactly<ArgumentException>();
        }

        [TestMethod]
        [DataRow(typeof(int))]
        [DataRow(typeof(float))]
        [DataRow(typeof(long))]
        [DataRow(typeof(BigInteger))]
        [DataRow(typeof(Int128))]
        [DataRow(typeof(Half))]
        [DataRow(typeof(Nullable<int>))]
        public void IsNumberic_NumericType_ReturnsTrue(Type type)
        {
            MathHelperAL.IsNumberic(type).Should().BeTrue();
        }

        [TestMethod]
        [DataRow(typeof(string))]
        [DataRow(typeof(DataRowAttribute))]
        [DataRow(typeof(char))]
        public void IsNumberic_NotNumericType_ReturnsFalse(Type type)
        {
            MathHelperAL.IsNumberic(type).Should().BeFalse();
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(-1)]
        [DataRow(0.1f)]
        [DataRow(0.1d)]
        [DataRow(10000000000L)]
        [DataRow(10000000000d)]
        public void IsNumeric_NumericValue_ReturnsTrue(object? numericValue)
        {
            MathHelperAL.IsNumberic(numericValue).Should().BeTrue();
        }

        [TestMethod]
        [DataRow("string")]
        [DataRow(new int[] { 1, 1 })]
        [DataRow('c')]
        [DataRow(null)]
        public void IsNumberic_NotNumericValue_ReturnsFalse(object? notNumericValue)
        {
            MathHelperAL.IsNumberic(notNumericValue).Should().BeFalse();
        }

        [TestMethod]
        public void ToBigInteger_Int128()
        {
            Int128 value = 12345678901234567890;
            MathHelperAL.ToBigInteger(value).Should().Be((BigInteger)value);
        }

        [TestMethod]
        public void ToBigInteger_UInt128()
        {
            UInt128 value = 12345678901234567890U;
            MathHelperAL.ToBigInteger(value).Should().Be((BigInteger)value);
        }

        [TestMethod]
        public void ToBigInteger_Int()
        {
            MathHelperAL.ToBigInteger(123).Should().Be(new BigInteger(123));
        }

        [TestMethod]
        public void ToBigInteger_InvalidType_ThrowsEx()
        {
            Invoking(() => MathHelperAL.ToBigInteger("test")).Should().ThrowExactly<ArgumentException>();
        }

        [TestMethod]
        [DataRow(5.05f)]
        [DataRow(5.01f)]
        [DataRow(5.09f)]
        public void IsNear_NearValue_ReturnsTrue(float value)
        {
            MathHelperAL.IsNear(5f, value, 0.1f).Should().BeTrue();
        }

        [TestMethod]
        [DataRow(59f)]
        [DataRow(6f)]
        public void IsNear_NotNearValue_ReturnsFalse(float value)
        {
            MathHelperAL.IsNear(5f, value, 0.1f).Should().BeFalse();
        }

        [TestMethod]
        public void AddPostfix_5000()
        {
            MathHelperAL.AddPostfix(5000).Should().Be("5k");
        }

        [TestMethod]
        public void AddPostfix_500000000()
        {
            MathHelperAL.AddPostfix(500_000_000).Should().Be("500M");
        }
    }
}