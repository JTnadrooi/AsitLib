using AsitLib.CommandLine;

namespace AsitLib.Tests
{
    [TestClass]
    public class ParseHelperTests
    {
        #region TO_KEBAB_CASE

        [TestMethod]
        public void ToKebabCase_PascalCase_ReturnsKebabCase()
        {
            Assert.AreEqual("hello-world", "HelloWorld".ToKebabCase());
        }

        [TestMethod]
        public void ToKebabCase_MixedCase_ReturnsLowercaseKebabCase()
        {
            Assert.AreEqual("http-request", "HTTPRequest".ToKebabCase());
        }

        [TestMethod]
        public void ToKebabCase_SingleWord_ReturnsLowercaseWord()
        {
            Assert.AreEqual("word", "Word".ToKebabCase());
        }

        #endregion

        #region PARSEHELPER_SPLIT

        [TestMethod]
        public void Split_SimpleWords_SplitsByWhitespace()
        {
            CollectionAssert.AreEqual(new string[] { "one", "two", "three" }, ParseHelper.Split("one two three"));
        }

        [TestMethod]
        public void Split_QuotedString_TreatsQuotesAsOneToken()
        {
            CollectionAssert.AreEqual(new string[] { "one", "two three", "four" }, ParseHelper.Split("one \"two three\" four"));
        }

        [TestMethod]
        public void Split_EscapedQuote_HandlesProperly()
        {
            CollectionAssert.AreEqual(new string[] { "one", "\"two", "three\"", "four" }, ParseHelper.Split("one \\\"two three\\\" four"));
        }

        [TestMethod]
        public void Split_ExtraSpaces_IgnoresThemOutsideQuotes()
        {
            CollectionAssert.AreEqual(new string[] { "one", "two" }, ParseHelper.Split("  one   two   "));
        }

        [TestMethod]
        public void Split_EmptyString_ReturnsEmptyArray()
        {
            CollectionAssert.AreEqual(Array.Empty<string>(), ParseHelper.Split(string.Empty));
        }

        #endregion
    }
}
