
namespace AsitLib.Tests
{
    [TestClass]
    public class GlobalOptionTests
    {
        [TestMethod]
        public void Ctor_OptionWithoutName_ThrowsEx()
        {
            Invoking(() => new DummyGlobalOption(OptionInfo.FromType(typeof(bool)), "desc")).Should().ThrowExactly<ArgumentException>();
        }
    }
}
