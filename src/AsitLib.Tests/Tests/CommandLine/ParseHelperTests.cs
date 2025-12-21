namespace AsitLib.Tests
{
    [TestClass]
    public class ParseHelperTests
    {
        #region PARSE_SIGNATURE

        [TestMethod]
        public void ParseSignature_PascalCase_ReturnsKebabCase()
        {
            ParseHelpers.GetSignature("HelloWorld").Should().Be("hello-world");
        }

        [TestMethod]
        public void ParseSignature_MixedCase_ReturnsLowercaseKebabCase()
        {
            ParseHelpers.GetSignature("HTTPRequest").Should().Be("http-request");
        }

        [TestMethod]
        public void ParseSignature_SingleWord_ReturnsLowercaseWord()
        {
            ParseHelpers.GetSignature("Word").Should().Be("word");
        }

        #endregion

        #region SPLIT

        [TestMethod]
        public void Split_SimpleWords_SplitsByWhitespace()
        {
            ParseHelpers.Split("one two three").Should().Equal(new string[] { "one", "two", "three" });
        }

        [TestMethod]
        public void Split_QuotedString_TreatsQuotesAsOneToken()
        {
            ParseHelpers.Split("one \"two three\" four").Should().Equal(new string[] { "one", "two three", "four" });
        }

        [TestMethod]
        public void Split_EscapedQuote_HandlesProperly()
        {
            ParseHelpers.Split("one \\\"two three\\\" four").Should().Equal(new string[] { "one", "\"two", "three\"", "four" });
        }

        [TestMethod]
        public void Split_ExtraSpaces_IgnoresThemOutsideQuotes()
        {
            ParseHelpers.Split("  one   two  ").Should().Equal(new string[] { "one", "two" });
            ParseHelpers.Split("  one   two  \"   \"").Should().Equal(new string[] { "one", "two", "   " });
        }

        [TestMethod]
        public void Split_EmptyString_ReturnsEmptyArray()
        {
            ParseHelpers.Split(string.Empty).Should().Equal(Array.Empty<string>());
        }

        [TestMethod]
        public void Split_Single_ReturnsInput()
        {
            ParseHelpers.Split("hello").Should().Equal(new string[] { "hello" });
        }

        #endregion

        #region CONVERT

        [TestMethod]
        public void Convert_EnumValueInt_GetsEnumEntryWithValue()
        {
            ParseHelpers.GetValue("2", typeof(TestEnum)).Should().Be(TestEnum.Value2);
        }

        [TestMethod]
        public void Convert_EnumValueString_GetsEnumEntryWithValue()
        {
            ParseHelpers.GetValue("value-2", typeof(TestEnum)).Should().Be(TestEnum.Value2);
        }

        [TestMethod]
        public void Convert_CustomSignatureEnumValueString_GetsCustomNameEnumEntryWithValue()
        {
            ParseHelpers.GetValue("three", typeof(TestEnum)).Should().Be(TestEnum.Value3);
        }

        [TestMethod]
        public void Convert_MultipleIntTokens_ParsesToIntArray()
        {
            ((int[])ParseHelpers.GetValue(new[] { "4", "2" }, typeof(int[]))!).Should().Equal(new int[] { 4, 2 });
        }

        [TestMethod]
        public void Convert_MultipleStringTokens_ParsesToStringArray()
        {
            ((string[])ParseHelpers.GetValue(new[] { "e", "a" }, typeof(string[]))!).Should().Equal(new string[] { "e", "a" });
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
            ParseHelpers.IsValidGenericFlagCall(input).Should().BeFalse();
        }

        [TestMethod]
        [DataRow("--help")]
        [DataRow("-h")]
        public void IsValidGenericFlagCall_ValidSignature_ReturnsTrue(string input)
        {
            ParseHelpers.IsValidGenericFlagCall(input).Should().BeTrue();
        }

        [TestMethod]
        public void GetGenericFlagSignature_InputLongForm()
        {
            ParseHelpers.GetGenericFlagSignature("help").Should().Be("--help");
        }

        [TestMethod]
        public void GetGenericFlagSignature_InputShorthand()
        {
            ParseHelpers.GetGenericFlagSignature("h").Should().Be("-h");
        }

        [TestMethod]
        [DataRow("--help")]
        [DataRow("-h")]
        public void GetGenericFlagSignature_AlreadyFlag_ThrowsEx(string input)
        {
            Invoking(() => ParseHelpers.GetGenericFlagSignature(input)).Should().Throw<InvalidOperationException>();
        }
    }
}
