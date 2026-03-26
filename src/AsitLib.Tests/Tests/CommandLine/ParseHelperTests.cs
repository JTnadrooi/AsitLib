namespace AsitLib.Tests
{
    [TestClass]
    public class ParseHelperTests
    {
        #region GET_SIGNATURE

        [TestMethod]
        public void GetSignature_PascalCase_ReturnsKebabCase()
        {
            ParseHelpers.GetSignature("HelloWorld").Should().Be("hello-world");
        }

        [TestMethod]
        public void GetSignature_MixedCase_ReturnsLowercaseKebabCase()
        {
            ParseHelpers.GetSignature("HTTPRequest").Should().Be("http-request");
        }

        [TestMethod]
        public void GetSignature_SingleWord_ReturnsLowercaseWord()
        {
            ParseHelpers.GetSignature("Word").Should().Be("word");
        }

        #endregion

        #region GET_TOKENS

        [TestMethod]
        public void GetTokens_SimpleWords_SplitsByWhitespace()
        {
            ParseHelpers.GetTokens("one two three").Should().Equal(new string[] { "one", "two", "three" });
        }

        [TestMethod]
        public void GetTokens_QuotedString_TreatsQuotesAsOneToken()
        {
            ParseHelpers.GetTokens("one \"two three\" four").Should().Equal(new string[] { "one", "\"two three\"", "four" });
        }

        [TestMethod]
        public void GetTokens_MultipleQuotedSections()
        {
            ParseHelpers.GetTokens("one \"two three\" \"four 4\"").Should().Equal(new string[] { "one", "\"two three\"", "\"four 4\"" });
        }

        [TestMethod]
        public void GetTokens_EscapedQuotes()
        {
            ParseHelpers.GetTokens("one \\\"two three\\\" four").Should().Equal(new string[] { "one", "\"two", "three\"", "four" });
        }

        [TestMethod]
        public void GetTokens_ExtraSpacesExcludeQuotes_IgnoresThemOutsideQuotes()
        {
            ParseHelpers.GetTokens("  one   two  ").Should().Equal(new string[] { "one", "two" });
            ParseHelpers.GetTokens("  one   two  \"   \"").Should().Equal(new string[] { "one", "two", "\"   \"" });
        }

        [TestMethod]
        public void GetTokens_EmptyString_ReturnsEmptyArray() // like with Split()
        {
            ParseHelpers.GetTokens(string.Empty).Should().Equal(Array.Empty<string>());
        }

        [TestMethod]
        public void GetTokens_Single_ReturnsInput()
        {
            ParseHelpers.GetTokens("hello").Should().Equal(new string[] { "hello" });
        }

        #endregion

        #region UNQUOTE

        [TestMethod]
        public void UnQuote_OuterQuotes_ReturnsStringWithoutOuterQuotes()
        {
            ParseHelpers.UnQuote("\"string\"").Should().Be("string"); // input: "string", output: string
        }

        [TestMethod]
        public void UnQuote_EscapedOuterQuotes_ReturnsStringWithEscapedOuterQuotes()
        {
            ParseHelpers.UnQuote("\\\"string\\\"").Should().Be("\\\"string\\\""); // input: \"string\", output: \"string\"
        }

        [TestMethod]
        public void UnQuote_WhenStringHasNoOuterQuotes_ShouldReturnSameString()
        {
            ParseHelpers.UnQuote("string").Should().Be("string"); // input: string, output: string
        }

        [TestMethod]
        public void UnQuote_MidStringQuote_ShouldReturnSameString()
        {
            ParseHelpers.UnQuote("str\"ng").Should().Be("str\"ng"); // input: str"ng, output: str"ng
        }

        [TestMethod]
        public void UnQuote_StringWithInnerQuotes_ShouldOnlyRemoveOuterQuotes()
        {
            ParseHelpers.UnQuote("\"hello \\\"world\\\"\"").Should().Be("hello \\\"world\\\""); // input: "hello \"world\"", output: hello \"world\"
        }

        [TestMethod]
        public void UnQuote_EmptyString_ShouldReturnEmptyString()
        {
            ParseHelpers.UnQuote(string.Empty).Should().BeEmpty(); // input: (empty string), output: (empty string)
        }

        [TestMethod]
        public void UnQuote_TwoQuotes_ShouldReturnEmptyString()
        {
            ParseHelpers.UnQuote("\"\"").Should().BeEmpty(); // input: "", output: (empty string)
        }

        [TestMethod]
        public void UnQuote_ThreeQuotes_ShouldReturnInBetweenQuotes()
        {
            ParseHelpers.UnQuote("\"\"\"").Should().Be("\""); // input: """, output: "
        }

        [TestMethod]
        public void WhenInputIsSingleCharacter_ShouldReturnSameCharacter()
        {
            ParseHelpers.UnQuote("a").Should().Be("a"); // input: a, output: a
        }

        [TestMethod]
        [DataRow("hello world\"")] // hello world"
        [DataRow("\"hello world")] // "hello world
        [DataRow("\"\"\\\"")] // ""\"
        [DataRow("\"")] // "
        //[DataRow("\"\"\"")] // """
        public void WhenQuotesAreMismatched_ThrowEx(string stringWithMismatchedQuotes)
        {
            Invoking(() => ParseHelpers.UnQuote(stringWithMismatchedQuotes)).Should().ThrowExactly<ArgumentException>(because: "quotes are mismatched");
        }

        [TestMethod]
        public void WhenQuotedStringHasWhitespace_ShouldRemoveQuotesAndPreserveWhitespace()
        {
            ParseHelpers.UnQuote("\"  hello world  \"").Should().Be("  hello world  "); // input: "  hello world  ", output:   hello world  
        }

        #endregion

        [TestMethod]
        [DataRow("-help")]
        [DataRow("---help")]
        [DataRow("---h")]
        [DataRow("--h")]
        [DataRow("--?")]
        [DataRow("h")]
        [DataRow("help")]
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
            Invoking(() => ParseHelpers.GetGenericFlagSignature(input)).Should().ThrowExactly<ArgumentException>();
        }
    }
}
