using AsitLib.CommandLine;

namespace AsitLib.Tests
{
    [TestClass]
    public class ParseHelperTests
    {
        #region PARSE_SIGNATURE

        [TestMethod]
        public void ParseSignature_PascalCase_ReturnsKebabCase()
        {
            Assert.AreEqual("hello-world", ParseHelpers.ParseSignature("HelloWorld"));
        }

        [TestMethod]
        public void ParseSignature_MixedCase_ReturnsLowercaseKebabCase()
        {
            Assert.AreEqual("http-request", ParseHelpers.ParseSignature("HTTPRequest"));
        }

        [TestMethod]
        public void ParseSignature_SingleWord_ReturnsLowercaseWord()
        {
            Assert.AreEqual("word", ParseHelpers.ParseSignature("Word"));
        }

        #endregion

        #region PARSEHELPER_SPLIT

        [TestMethod]
        public void Split_SimpleWords_SplitsByWhitespace()
        {
            CollectionAssert.AreEqual(new string[] { "one", "two", "three" }, ParseHelpers.Split("one two three"));
        }

        [TestMethod]
        public void Split_QuotedString_TreatsQuotesAsOneToken()
        {
            CollectionAssert.AreEqual(new string[] { "one", "two three", "four" }, ParseHelpers.Split("one \"two three\" four"));
        }

        [TestMethod]
        public void Split_EscapedQuote_HandlesProperly()
        {
            CollectionAssert.AreEqual(new string[] { "one", "\"two", "three\"", "four" }, ParseHelpers.Split("one \\\"two three\\\" four"));
        }

        [TestMethod]
        public void Split_ExtraSpaces_IgnoresThemOutsideQuotes()
        {
            CollectionAssert.AreEqual(new string[] { "one", "two" }, ParseHelpers.Split("  one   two   "));
        }

        [TestMethod]
        public void Split_EmptyString_ReturnsEmptyArray()
        {
            CollectionAssert.AreEqual(Array.Empty<string>(), ParseHelpers.Split(string.Empty));
        }

        #endregion
    }
}
