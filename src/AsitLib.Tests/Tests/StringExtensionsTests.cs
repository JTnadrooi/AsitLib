namespace AsitLib.Tests
{
    [TestClass]
    public class StringExtensionsTests
    {
        [TestMethod]
        public void FirstLine_ShouldReturnFirstLineOfString()
        {
            string input = "First line\nSecond line\nThird line";

            input.FirstLine().Should().Be("First line");
        }

        [TestMethod]
        public void FirstLine_ShouldReturnNull_WhenStringIsEmpty()
        {
            Invoking(() => string.Empty.FirstLine()).Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
        public void Between_ShouldReturnStringBetweenFirstStrings()
        {
            string input = "Hello [start]Content[end] World!";
            string result = input.Between("[start]", "[end]", BetweenMethod.FirstFirst);

            result.Should().Be("Content");
        }

        [TestMethod]
        public void Between_ShouldReturnStringBetweenFirstAndLastStrings()
        {
            string input = "Hello [start]Content1[end] More [start]Content2[end] World!";
            string result = input.Between("[start]", "[end]", BetweenMethod.FirstLast);

            result.Should().Be("Content1[end] More [start]Content2");
        }

        [TestMethod]
        public void Between_ShouldReturnStringBetweenLastStrings()
        {
            string input = "Hello [start]Content1[end] More [start]Content2[end] World!";
            string result = input.Between("[start]", "[end]", BetweenMethod.LastLast);

            result.Should().Be("Content2");
        }

        [TestMethod]
        public void Betweens_ShouldReturnMultipleMatches()
        {
            string input = "Hello [start]Content1[end] More [start]Content2[end] World!";
            string[] result = input.Betweens("[start]", "[end]");

            result.Should().Equal(["Content1", "Content2"]);
        }

        [TestMethod]
        public void ReplaceFirst_ShouldReplaceFirstOccurrence()
        {
            string input = "Hello [start]Content[end] World!";
            string result = input.ReplaceFirst("[start]", "[begin]");

            result.Should().Be("Hello [begin]Content[end] World!");
        }

        [TestMethod]
        public void ReplaceFirst_ShouldNotReplace_WhenSubstringNotFound()
        {
            string input = "Hello Content World!";
            string result = input.ReplaceFirst("[start]", "[begin]");

            result.Should().Be(input);
        }

        [TestMethod]
        public void SafeIntParse_ShouldReturnValidInt()
        {
            string input = "123";

            int result = input.SafeIntParse();

            result.Should().Be(123);
        }

        [TestMethod]
        public void SafeIntParse_ShouldReturnMinusOne_WhenParseFails()
        {
            string input = "abc";

            input.SafeIntParse().Should().Be(-1);
        }

        [TestMethod]
        public void SafeNullIntParse_ShouldReturnValidInt()
        {
            string input = "123";

            input.SafeNullIntParse().Should().Be(123);
        }

        [TestMethod]
        public void SafeNullIntParse_ShouldReturnNull_WhenParseFails()
        {
            string input = "abc";

            input.SafeNullIntParse().Should().BeNull();
        }

        [TestMethod]
        public void SafeNullBoolParse_ShouldReturnTrue_WhenTrueString()
        {
            string input = "true";

            input.SafeNullBoolParse().Should().BeTrue();
        }

        [TestMethod]
        public void SafeNullBoolParse_ShouldReturnFalse_WhenFalseString()
        {
            string input = "false";

            input.SafeNullBoolParse().Should().BeFalse();
        }

        [TestMethod]
        public void SafeNullBoolParse_ShouldReturnNull_WhenInvalidString()
        {
            string input = "invalid";

            input.SafeNullBoolParse().Should().BeNull();
        }
    }
}
