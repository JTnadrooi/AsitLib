using AsitLib.Numerics;
using System;
using System.Numerics;

namespace AsitLib.Tests
{
    [TestClass]
    public class MathALTests
    {
        [TestMethod]
        public void Invert_ValidValue()
        {
            float result = MathAL.Invert(5f, 0f);
            Assert.AreEqual(-5f, result);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Invert_InvalidValueNaN_ThrowsEx()
        {
            MathAL.Invert(float.NaN, 0f);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Invert_InvalidValueInf_ThrowsEx()
        {
            MathAL.Invert(float.PositiveInfinity, 0f);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Invert_InvalidValueNegInf_ThrowsEx()
        {
            MathAL.Invert(float.NegativeInfinity, 0f);
        }

        [TestMethod]
        public void Normalize_Degrees_ValidValue()
        {
            float result = MathAL.Normalize(370f);
            Assert.AreEqual(10f, result);
        }

        [TestMethod]
        public void Normalize_Radians_ValidValue()
        {
            float result = MathAL.Normalize((float)Math.PI * 2 + 0.5f, true);
            Assert.AreEqual(0.5f, result, 0.0001f);
        }

        [TestMethod]
        public void Difference_ValidValues()
        {
            float result = MathAL.Difference(10f, 5f);
            Assert.AreEqual(5f, result);
        }

        [TestMethod]
        public void Average_ValidValues()
        {
            float result = MathAL.Average(10f, 20f, 30f);
            Assert.AreEqual(20f, result);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Average_InvalidValueNaN_ThrowsEx()
        {
            MathAL.Average(10f, 20f, float.NaN);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Average_InvalidValueInf_ThrowsEx()
        {
            MathAL.Average(10f, float.PositiveInfinity, 20f);
        }

        [TestMethod]
        public void IsNumberic_TypeValid()
        {
            bool result = MathHelperAL.IsNumberic(typeof(int));
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsNumberic_TypeInvalid()
        {
            bool result = MathHelperAL.IsNumberic(typeof(string));
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsNumberic_ObjectValid()
        {
            bool result = MathHelperAL.IsNumberic(10);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsNumberic_ObjectInvalid()
        {
            bool result = MathHelperAL.IsNumberic("test");
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ToBigInteger_Int128()
        {
            Int128 value = 12345678901234567890;
            BigInteger result = MathHelperAL.ToBigInteger(value);
            Assert.AreEqual((BigInteger)value, result);
        }

        [TestMethod]
        public void ToBigInteger_UInt128()
        {
            UInt128 value = 12345678901234567890U;
            BigInteger result = MathHelperAL.ToBigInteger(value);
            Assert.AreEqual((BigInteger)value, result);
        }

        [TestMethod]
        public void ToBigInteger_Int()
        {
            int value = 123;
            BigInteger result = MathHelperAL.ToBigInteger(value);
            Assert.AreEqual(new BigInteger(123), result);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ToBigInteger_InvalidType_ThrowsEx()
        {
            string value = "test";
            MathHelperAL.ToBigInteger(value);
        }

        [TestMethod]
        public void IsNear_True()
        {
            bool result = MathHelperAL.IsNear(5f, 5.05f, 0.1f);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsNear_False()
        {
            bool result = MathHelperAL.IsNear(5f, 6f, 0.1f);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void AddPostfix_5000()
        {
            string result = MathHelperAL.AddPostfix(5000);
            Assert.AreEqual("5k", result);
        }

        [TestMethod]
        public void AddPostfix_500000000()
        {
            string result = MathHelperAL.AddPostfix(500_000_000);
            Assert.AreEqual("500M", result);
        }
    }
}
