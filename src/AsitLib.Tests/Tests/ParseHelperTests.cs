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

        #region CONVERT

        [TestMethod]
        public void Convert_EnumValueInt_GetsEnumEntryWithValue()
        {
            Assert.AreEqual(TestEnum.Value2, ParseHelpers.Convert("2", typeof(TestEnum)));
        }

        [TestMethod]
        public void Convert_EnumValueString_GetsEnumEntryWithValue()
        {
            Assert.AreEqual(TestEnum.Value2, ParseHelpers.Convert("value-2", typeof(TestEnum)));
        }

        [TestMethod]
        public void Convert_CustomSignatureEnumValueString_GetsCustomNameEnumEntryWithValue()
        {
            Assert.AreEqual(TestEnum.Value3, ParseHelpers.Convert("three", typeof(TestEnum)));
        }

        [TestMethod]
        public void Convert_MultipleIntTokens_ParsesToIntArray()
        {
            CollectionAssert.AreEqual(new int[] { 4, 2 }, (int[])ParseHelpers.Convert(["4", "2"], typeof(int[]))!);
        }

        [TestMethod]
        public void Convert_MultipleStringTokens_ParsesToStringArray()
        {
            CollectionAssert.AreEqual(new string[] { "e", "a" }, (string[])ParseHelpers.Convert(["e", "a"], typeof(string[]))!);
        }

        #endregion
    }
}
