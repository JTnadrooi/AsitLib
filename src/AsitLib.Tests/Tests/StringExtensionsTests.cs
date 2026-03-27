namespace AsitLib.Tests
{
    [TestClass]
    public class StringExtensionsTests
    {
        [TestMethod]
        public void FirstLine_Multiline_ReturnsFirstLine()
        {
            string input = "First line\nSecond line\nThird line";

            input.FirstLine().Should().Be("First line");
        }

        [TestMethod]
        public void FirstLine_EmptyString_ReturnsEmpty()
        {
            string.Empty.FirstLine().Should().Be(string.Empty);
        }

        [TestMethod]
        public void Between_FirstFirst_ReturnsStringBetweenFirstStrings()
        {
            string input = "Hello [start]Content[end] World!";
            string result = input.Between("[start]", "[end]", BetweenMethod.FirstFirst);

            result.Should().Be("Content");
        }

        [TestMethod]
        public void Between_FirstLast_ReturnsStringBetweenFirstAndLastStrings()
        {
            string input = "Hello [start]Content1[end] More [start]Content2[end] World!";
            string result = input.Between("[start]", "[end]", BetweenMethod.FirstLast);

            result.Should().Be("Content1[end] More [start]Content2");
        }

        [TestMethod]
        public void Between_LastLast_ReturnsStringBetweenLastStrings()
        {
            string input = "Hello [start]Content1[end] More [start]Content2[end] World!";
            string result = input.Between("[start]", "[end]", BetweenMethod.LastLast);

            result.Should().Be("Content2");
        }

        [TestMethod]
        public void Betweens_MultipleMatches_ReturnsAllMatches()
        {
            string input = "Hello [start]Content1[end] More [start]Content2[end] World!";
            string[] result = input.Betweens("[start]", "[end]");

            result.Should().Equal(new[] { "Content1", "Content2" });
        }

        [TestMethod]
        public void ReplaceFirst_Found_ReplacesFirstOccurrence()
        {
            string input = "Hello [start]Content[end] World!";
            string result = input.ReplaceFirst("[start]", "[begin]");

            result.Should().Be("Hello [begin]Content[end] World!");
        }

        [TestMethod]
        public void ReplaceFirst_NotFound_DoesNotReplace()
        {
            string input = "Hello Content World!";
            string result = input.ReplaceFirst("[start]", "[begin]");

            result.Should().Be(input);
        }

        [TestMethod]
        public void SafeIntParse_ValidInteger_ReturnsParsedValue()
        {
            string input = "123";

            int result = input.SafeIntParse();

            result.Should().Be(123);
        }

        [TestMethod]
        public void SafeIntParse_InvalidInteger_ReturnsMinusOne()
        {
            "abc".SafeIntParse().Should().Be(-1);
        }

        [TestMethod]
        public void SafeNullIntParse_ValidInteger_ReturnsParsedValue()
        {
            "123".SafeNullIntParse().Should().Be(123);
        }

        [TestMethod]
        public void SafeNullIntParse_InvalidInteger_ReturnsNull()
        {
            "abc".SafeNullIntParse().Should().BeNull();
        }

        [TestMethod]
        public void SafeNullBoolParse_TrueString_ReturnsTrue()
        {
            "true".SafeNullBoolParse().Should().BeTrue();
        }

        [TestMethod]
        public void SafeNullBoolParse_FalseString_ReturnsFalse()
        {
            "false".SafeNullBoolParse().Should().BeFalse();
        }

        [TestMethod]
        public void SafeNullBoolParse_InvalidString_ReturnsNull()
        {
            "invalid".SafeNullBoolParse().Should().BeNull();
        }
    }
}
