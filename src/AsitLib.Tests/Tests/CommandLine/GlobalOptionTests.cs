
namespace AsitLib.Tests
{
    [TestClass]
    public class GlobalOptionTests
    {
        [TestMethod]
        public void Ctor_OptionWithoutId_ThrowsEx()
        {
            Invoking(() => new DummyGlobalOption(OptionInfo.FromType(typeof(bool)), "desc")).Should().ThrowExactly<ArgumentException>();
        }

        [TestMethod]
        public void Ctor_OptionIdSetsActionHookId()
        {
            new DummyGlobalOption(OptionInfo.FromType(typeof(bool), "option"), "desc").Id.Should().Be("option");
        }

        [TestMethod]
        public void Ctor_IdOverrideSetsActionHookId()
        {
            new DummyGlobalOption(OptionInfo.FromType(typeof(bool), "option"), "desc", "override").Id.Should().Be("override");
        }
    }
}
