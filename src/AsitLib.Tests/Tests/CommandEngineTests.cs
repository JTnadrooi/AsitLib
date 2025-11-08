using AsitLib.CommandLine;
using System.Reflection;

namespace AsitLib.Tests
{
    public class TestCommandProvider : CommandProvider
    {
        public TestCommandProvider() : base("test") { }

        [Command("desc", inheritNamespace: false)]
        public string Print(string input, bool upperCase = false)
        {
            return upperCase ? input.ToUpper() : input;
        }

        [Command("desc", inheritNamespace: false)]
        public string Tv([AllowAntiArgumentAttribute] bool color = true)
        {
            return "Tv" + (color ? " in color!" : ".");
        }

        [Command("desc", inheritNamespace: false, aliases: ["hi"])]
        public string Greet([ParameterName("name")] string yourName)
        {
            return $"Hi, {yourName}!";
        }
    }

    [TestClass]
    public class CommandEngineTests
    {
        public static void AssertExecute(string expected, string args) => AssertExecute(expected, ParseHelpers.Split(args));
        public static void AssertExecute(string expected, string[] args)
        {
            Assert.AreEqual(expected, AsitLibTests.Engine.ExecuteAndCapture(args));
        }

        [TestMethod]
        public void PrintCommand_IndexedArgument_GetsParameterFromIndex()
        {
            AssertExecute("hello", "print hello");
        }

        [TestMethod]
        public void PrintCommand_CamelCaseArgument_HandlesCorrectly()
        {
            AssertExecute("HELLO", "print hello --upper-case true");
        }

        [TestMethod]
        public void PrintCommand_NoValueArgument_ActsAsTrue()
        {
            AssertExecute("HELLO", "print hello --upper-case");
        }

        [TestMethod]
        public void TvCommand_NoValueAntiArgument_ActsAsFalse()
        {
            AssertExecute("Tv.", "tv --no-color");
        }

        [TestMethod]
        public void ParameterNameAttribute_Read_OverwritesParameterName()
        {
            AssertExecute("Hi, me!", "greet --name me");
        }

        [TestMethod]
        public void CommandAlias_CallAlias_CallsCommandWithAlias()
        {
            AssertExecute("Hi, myself!", "hi myself");
        }
    }
}
