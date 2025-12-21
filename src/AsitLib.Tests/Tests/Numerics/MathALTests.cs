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
        public void Invert_InvalidValueNaN_ThrowsEx()
        {
            Invoking(() => MathAL.Invert(float.NaN, 0f)).Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
        public void Invert_InvalidValueInf_ThrowsEx()
        {
            Invoking(() => MathAL.Invert(float.PositiveInfinity, 0f)).Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
        public void Invert_InvalidValueNegInf_ThrowsEx()
        {
            Invoking(() => MathAL.Invert(float.NegativeInfinity, 0f)).Should().Throw<InvalidOperationException>();
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
        public void Average_InvalidValueNaN_ThrowsEx()
        {
            Invoking(() => MathAL.Average(10f, 20f, float.NaN)).Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
        public void Average_InvalidValueInf_ThrowsEx()
        {
            Invoking(() => MathAL.Average(10f, float.PositiveInfinity, 20f)).Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
        public void IsNumberic_TypeValid()
        {
            MathHelperAL.IsNumberic(typeof(int)).Should().BeTrue();
        }

        [TestMethod]
        public void IsNumberic_TypeInvalid()
        {
            MathHelperAL.IsNumberic(typeof(string)).Should().BeFalse();
        }

        [TestMethod]
        public void IsNumberic_ObjectValid()
        {
            MathHelperAL.IsNumberic(10).Should().BeTrue();
        }

        [TestMethod]
        public void IsNumberic_ObjectInvalid()
        {
            MathHelperAL.IsNumberic("test").Should().BeFalse();
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
            Invoking(() => MathHelperAL.ToBigInteger("test")).Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
        public void IsNear_True()
        {
            MathHelperAL.IsNear(5f, 5.05f, 0.1f).Should().BeTrue();
        }

        [TestMethod]
        public void IsNear_False()
        {
            MathHelperAL.IsNear(5f, 6f, 0.1f).Should().BeFalse();
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