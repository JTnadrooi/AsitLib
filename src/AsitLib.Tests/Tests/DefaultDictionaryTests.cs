namespace AsitLib.Tests
{
    [TestClass]
    public class DefaultDictionaryTests
    {
        [TestMethod]
        public void TryGetValue_FoundKey()
        {
            DefaultDictionary<int, string> dictionary = new DefaultDictionary<int, string>(() => "default");
            dictionary[1] = "custom value";
            bool result = dictionary.TryGetValue(1, out string value);

            result.Should().BeTrue();
            value.Should().Be("custom value");
        }

        [TestMethod]
        public void TryGetValue_NotFoundKey_AlwaysSucceeds()
        {
            DefaultDictionary<int, string> dictionary = new DefaultDictionary<int, string>(() => "default");
            bool result = dictionary.TryGetValue(1, out string value);

            result.Should().BeTrue();
            value.Should().Be("default");
        }

        [TestMethod]
        public void GetValue_FoundKey()
        {
            DefaultDictionary<int, string> dictionary = new DefaultDictionary<int, string>(() => "default");
            dictionary[1] = "custom value";

            dictionary[1].Should().Be("custom value");
        }

        [TestMethod]
        public void GetValue_NotFoundKey()
        {
            DefaultDictionary<int, string> dictionary = new DefaultDictionary<int, string>(() => "default");

            dictionary[1].Should().Be("default");
        }
    }
}
