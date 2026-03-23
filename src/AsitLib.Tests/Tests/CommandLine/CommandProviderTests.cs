namespace AsitLib.Tests
{
    [TestClass]
    public class CommandProviderTests
    {
        [TestMethod]
        [DataRow(" name ")]
        [DataRow(" name")]
        [DataRow("na me")]
        [DataRow("-name")]
        [DataRow("")]
        [DataRow(" ")]
        [DataRow("      ")]
        public void Contruct_WithInvalidName_ThrowsEx(string name)
        {
            Invoking(() => new DummyCommandProvider(name)).Should().ThrowExactly<ArgumentException>();
        }
    }
}
