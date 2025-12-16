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
            Assert.AreEqual("hello-world", ParseHelpers.GetSignature("HelloWorld"));
        }

        [TestMethod]
        public void ParseSignature_MixedCase_ReturnsLowercaseKebabCase()
        {
            Assert.AreEqual("http-request", ParseHelpers.GetSignature("HTTPRequest"));
        }

        [TestMethod]
        public void ParseSignature_SingleWord_ReturnsLowercaseWord()
        {
            Assert.AreEqual("word", ParseHelpers.GetSignature("Word"));
        }

        #endregion

        #region SPLIT

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
            CollectionAssert.AreEqual(new string[] { "one", "two" }, ParseHelpers.Split("  one   two  "));
            CollectionAssert.AreEqual(new string[] { "one", "two", "   " }, ParseHelpers.Split("  one   two  \"   \""));
        }

        [TestMethod]
        public void Split_EmptyString_ReturnsEmptyArray()
        {
            CollectionAssert.AreEqual(Array.Empty<string>(), ParseHelpers.Split(string.Empty));
        }

        [TestMethod]
        public void Split_Single_ReturnsInput()
        {
            CollectionAssert.AreEqual(new string[] { "hello" }, ParseHelpers.Split("hello"));
        }

        #endregion

        #region CONVERT

        [TestMethod]
        public void Convert_EnumValueInt_GetsEnumEntryWithValue()
        {
            Assert.AreEqual(TestEnum.Value2, ParseHelpers.GetValue("2", typeof(TestEnum)));
        }

        [TestMethod]
        public void Convert_EnumValueString_GetsEnumEntryWithValue()
        {
            Assert.AreEqual(TestEnum.Value2, ParseHelpers.GetValue("value-2", typeof(TestEnum)));
        }

        [TestMethod]
        public void Convert_CustomSignatureEnumValueString_GetsCustomNameEnumEntryWithValue()
        {
            Assert.AreEqual(TestEnum.Value3, ParseHelpers.GetValue("three", typeof(TestEnum)));
        }

        [TestMethod]
        public void Convert_MultipleIntTokens_ParsesToIntArray()
        {
            CollectionAssert.AreEqual(new int[] { 4, 2 }, (int[])ParseHelpers.GetValue(["4", "2"], typeof(int[]))!);
        }

        [TestMethod]
        public void Convert_MultipleStringTokens_ParsesToStringArray()
        {
            CollectionAssert.AreEqual(new string[] { "e", "a" }, (string[])ParseHelpers.GetValue(["e", "a"], typeof(string[]))!);
        }

        #endregion

        [TestMethod]
        [DataRow("-help")]
        [DataRow("---help")]
        [DataRow("---h")]
        [DataRow("--h")]
        [DataRow("--?")]
        [DataRow("h")]
        public void IsValidGenericFlagCall_InvalidSignature_ReturnsFalse(string input)
        {
            Assert.IsFalse(ParseHelpers.IsValidGenericFlagCall(input));
        }

        [TestMethod]
        [DataRow("--help")]
        [DataRow("-h")]
        public void IsValidGenericFlagCall_ValidSignature_ReturnsTrue(string input)
        {
            Assert.IsTrue(ParseHelpers.IsValidGenericFlagCall(input));
        }

        [TestMethod]
        public void GetGenericFlagSignature_InputLongForm()
        {
            Assert.AreEqual(ParseHelpers.GetGenericFlagSignature("help"), "--help");
        }

        [TestMethod]
        public void GetGenericFlagSignature_InputShorthand()
        {
            Assert.AreEqual(ParseHelpers.GetGenericFlagSignature("h"), "-h");
        }

        [TestMethod]
        [DataRow("--help")]
        [DataRow("-h")]
        public void GetGenericFlagSignature_AlreadyFlag_ThrowsEx(string input)
        {
            Assert.Throws<InvalidOperationException>(() => ParseHelpers.GetGenericFlagSignature(input));
        }
    }
}
