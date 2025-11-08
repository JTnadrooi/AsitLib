using AsitLib.CommandLine;
using System.Reflection;

namespace AsitLib.Tests
{
    [TestClass]
    public class CommandEngineTests
    {
        public static void AssertExecute(string expected, string args) => AssertExecute(expected, ParseHelpers.Split(args));
        public static void AssertExecute(string expected, string[] args)
        {
            Assert.AreEqual(expected, AsitLibTests.Engine.ExecuteAndCapture(args));
        }

        [TestMethod]
        public void PrintCommand_IndexedArgument_HandlesCorrectly()
        {
            AssertExecute("hello", "print hello");
        }

        [TestMethod]
        public void PrintCommand_CamelCaseArgument_HandlesCorrectly()
        {
            AssertExecute("HELLO", "print hello --upper-case true");
        }

        [TestMethod]
        public void PrintCommand_NoValueArgument_HandlesCorrectly()
        {
            AssertExecute("HELLO", "print hello --upper-case");
        }
    }
}
