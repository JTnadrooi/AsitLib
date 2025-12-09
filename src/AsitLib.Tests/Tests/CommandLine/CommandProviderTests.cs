using AsitLib.CommandLine;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace AsitLib.Tests
{
    public class DummyCommandProvider : CommandProvider
    {
        public DummyCommandProvider(string name) : base(name, null) { }
    }

    [TestClass]
    public class CommandProviderTests
    {
        [TestMethod]
        [DataRow("name#")]
        [DataRow("name-")]
        [DataRow(" name ")]
        [DataRow(" name")]
        [DataRow("na me")]
        [DataRow("-name")]
        [DataRow("")]
        [DataRow(" ")]
        [DataRow("      ")]
        public void Contruct_WithInvalidName_ThrowsEx(string name)
        {
            Assert.ThrowsException<InvalidOperationException>(() =>
                new DummyCommandProvider(name)
            );
        }
    }
}
