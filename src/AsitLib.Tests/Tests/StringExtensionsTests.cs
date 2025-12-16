using Microsoft.VisualStudio.TestTools.UnitTesting;
using AsitLib;
using System;
using System.Linq;

namespace AsitLib.Tests
{
    [TestClass]
    public class StringExtensionsTests
    {
        [TestMethod]
        public void FirstLine_ShouldReturnFirstLineOfString()
        {
            string input = "First line\nSecond line\nThird line";

            string result = input.FirstLine();

            Assert.AreEqual("First line", result);
        }

        [TestMethod]
        public void FirstLine_ShouldReturnNull_WhenStringIsEmpty()
        {
            Assert.Throws<InvalidOperationException>(() => string.Empty.FirstLine());
        }

        [TestMethod]
        public void Between_ShouldReturnStringBetweenFirstStrings()
        {
            string input = "Hello [start]Content[end] World!";
            string result = input.Between("[start]", "[end]", BetweenMethod.FirstFirst);

            Assert.AreEqual("Content", result);
        }

        [TestMethod]
        public void Between_ShouldReturnStringBetweenFirstAndLastStrings()
        {
            string input = "Hello [start]Content1[end] More [start]Content2[end] World!";
            string result = input.Between("[start]", "[end]", BetweenMethod.FirstLast);

            Assert.AreEqual("Content1[end] More [start]Content2", result);
        }

        [TestMethod]
        public void Between_ShouldReturnStringBetweenLastStrings()
        {
            string input = "Hello [start]Content1[end] More [start]Content2[end] World!";
            string result = input.Between("[start]", "[end]", BetweenMethod.LastLast);

            Assert.AreEqual("Content2", result);
        }

        [TestMethod]
        public void Betweens_ShouldReturnMultipleMatches()
        {
            string input = "Hello [start]Content1[end] More [start]Content2[end] World!";
            string[] result = input.Betweens("[start]", "[end]");

            CollectionAssert.AreEqual(new string[] { "Content1", "Content2" }, result);
        }

        [TestMethod]
        public void ReplaceFirst_ShouldReplaceFirstOccurrence()
        {
            string input = "Hello [start]Content[end] World!";
            string result = input.ReplaceFirst("[start]", "[begin]");

            Assert.AreEqual("Hello [begin]Content[end] World!", result);
        }

        [TestMethod]
        public void ReplaceFirst_ShouldNotReplace_WhenSubstringNotFound()
        {
            string input = "Hello Content World!";
            string result = input.ReplaceFirst("[start]", "[begin]");

            Assert.AreEqual(input, result);
        }

        [TestMethod]
        public void SafeIntParse_ShouldReturnValidInt()
        {
            string input = "123";

            int result = input.SafeIntParse();

            Assert.AreEqual(123, result);
        }

        [TestMethod]
        public void SafeIntParse_ShouldReturnMinusOne_WhenParseFails()
        {
            string input = "abc";

            int result = input.SafeIntParse();

            Assert.AreEqual(-1, result);
        }

        [TestMethod]
        public void SafeNullIntParse_ShouldReturnValidInt()
        {
            string input = "123";

            int? result = input.SafeNullIntParse();

            Assert.AreEqual(123, result);
        }

        [TestMethod]
        public void SafeNullIntParse_ShouldReturnNull_WhenParseFails()
        {
            string input = "abc";

            int? result = input.SafeNullIntParse();

            Assert.IsNull(result);
        }

        [TestMethod]
        public void SafeNullBoolParse_ShouldReturnTrue_WhenTrueString()
        {
            string input = "true";

            bool? result = input.SafeNullBoolParse();

            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void SafeNullBoolParse_ShouldReturnFalse_WhenFalseString()
        {
            string input = "false";

            bool? result = input.SafeNullBoolParse();

            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public void SafeNullBoolParse_ShouldReturnNull_WhenInvalidString()
        {
            string input = "invalid";

            bool? result = input.SafeNullBoolParse();

            Assert.IsNull(result);
        }
    }
}
