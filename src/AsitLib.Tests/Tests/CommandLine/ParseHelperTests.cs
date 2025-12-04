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
        public void IsValidGenericFlagCall_InvalidSignature_ReturnsFalse()
        {
            Assert.IsFalse(ParseHelpers.IsValidGenericFlagCall("-help"));
            Assert.IsFalse(ParseHelpers.IsValidGenericFlagCall("---help"));
            Assert.IsFalse(ParseHelpers.IsValidGenericFlagCall("---h"));
            Assert.IsFalse(ParseHelpers.IsValidGenericFlagCall("--h"));
            Assert.IsFalse(ParseHelpers.IsValidGenericFlagCall("--?"));
            Assert.IsFalse(ParseHelpers.IsValidGenericFlagCall("h"));
        }

        [TestMethod]
        public void IsValidGenericFlagCall_ValidSignature_ReturnsTrue()
        {
            Assert.IsTrue(ParseHelpers.IsValidGenericFlagCall("--help"));
            Assert.IsTrue(ParseHelpers.IsValidGenericFlagCall("-h"));
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
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetGenericFlagSignature_InputValidLongformFlagCall_ThrowsError()
        {
            ParseHelpers.GetGenericFlagSignature("--help");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetGenericFlagSignature_InputValidShorthandFlagCall_ThrowsError()
        {
            ParseHelpers.GetGenericFlagSignature("-h");
        }
    }
}
