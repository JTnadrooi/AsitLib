namespace AsitLib.Tests
{
    [TestClass]
    public class DefaultDictionaryTests
    {
        [TestMethod]
        public void TryGetValue_FoundKey_ReturnsTrueAndGetsValueFromKey()
        {
            DefaultDictionary<int, string> dictionary = new DefaultDictionary<int, string>(() => "default");
            dictionary[1] = "custom value";
            bool result = dictionary.TryGetValue(1, out string value);

            result.Should().BeTrue();
            value.Should().Be("custom value");
        }

        [TestMethod]
        public void TryGetValue_NotFoundKey_ReturnsTrueAndGetsDefaultValue()
        {
            DefaultDictionary<int, string> dictionary = new DefaultDictionary<int, string>(() => "default");
            bool result = dictionary.TryGetValue(1, out string value);

            result.Should().BeTrue();
            value.Should().Be("default");
        }

        [TestMethod]
        public void IndexerGet_FoundKey_ReturnsValueFromKey()
        {
            DefaultDictionary<int, string> dictionary = new DefaultDictionary<int, string>(() => "default");
            dictionary[1] = "custom value";

            dictionary[1].Should().Be("custom value");
        }

        [TestMethod]
        public void IndexerGet_NotFoundKey_ReturnsDefaultValue()
        {
            DefaultDictionary<int, string> dictionary = new DefaultDictionary<int, string>(() => "default");

            dictionary[1].Should().Be("default");
        }
    }
}
