using AsitLib.CommandLine;
using AsitLib.CommandLine.Attributes;
using System.Reflection;

namespace AsitLib.Tests
{
    public enum TestEnum
    {
        Value0 = 0,
        Value1 = 1,
        Value2 = 2,
        [CustomSignature("three")]
        Value3 = 3,
    }

    public class TestCommandProvider : CommandProvider
    {
        public TestCommandProvider() : base("test") { }

        [CommandAttribute("desc", inheritNamespace: false)]
        public string Print(string input, bool upperCase = false)
        {
            return upperCase ? input.ToUpper() : input;
        }

        [CommandAttribute("desc", inheritNamespace: false)]
        public string Tv([AllowAntiArgument] bool color = true)
        {
            return "Tv" + (color ? " in color!" : ".");
        }

        [CommandAttribute("desc", inheritNamespace: false, aliases: ["hi"])]
        public string Greet([CustomSignature("name")] string yourName)
        {
            return $"Hi, {yourName}!";
        }

        [CommandAttribute("desc", inheritNamespace: false)]
        public void Void()
        {

        }

        [CommandAttribute("desc", inheritNamespace: false)]
        public string Shorthand([Shorthand("wa")] string wayToLongParameterName, [Shorthand] int secondWayToLongOne = 0)
        {
            return wayToLongParameterName + " | " + secondWayToLongOne;
        }
    }

    [TestClass]
    public class CommandEngineTests
    {
        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            Engine = new CommandEngine()
                .RegisterProvider(new TestCommandProvider(), CommandInfoFactory.Default);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            Engine.Dispose();
        }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public static CommandEngine Engine { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        public static void AssertExecute(string expected, string args) => AssertExecute(expected, ParseHelpers.Split(args));
        public static void AssertExecute(string expected, string[] args)
        {
            Assert.AreEqual(expected, Engine.ExecuteAndCapture(args));
        }

        [TestMethod]
        public void Execute_IndexedArgument_GetsParameterFromIndex()
        {
            AssertExecute("hello", "print hello");
        }

        [TestMethod]
        public void Execute_CamelCaseArgument_HandlesCorrectly()
        {
            AssertExecute("HELLO", "print hello --upper-case true");
        }

        [TestMethod]
        public void Execute_NoValueArgument_ActsAsTrue()
        {
            AssertExecute("HELLO", "print hello --upper-case");
        }

        [TestMethod]
        public void Execute_NoValueAntiArgument_ActsAsFalse()
        {
            AssertExecute("Tv.", "tv --no-color");
        }

        [TestMethod]
        public void Execute_ParameterNameAttributeName_OverwritesParameterName()
        {
            AssertExecute("Hi, me!", "greet --name me");
        }

        [TestMethod]
        public void Execute_UseAlias_ExecutesCommandWithAlias()
        {
            AssertExecute("Hi, myself!", "hi myself");
        }

        [TestMethod]
        public void Execute_CommandUsingShorthandParameter_FindsCorrectParameter()
        {
            AssertExecute("shorter | 0", "shorthand -wa shorter");
        }

        [TestMethod]
        [ExpectedException(typeof(CommandException))]
        public void Execute_InvalidParameter_ThrowsCommandException()
        {
            Engine.Execute("print hi --doesnt-exist ahoy");
        }

        [TestMethod]
        [ExpectedException(typeof(CommandException))]
        public void Execute_MissingArgument_ThrowsCommandException()
        {
            Engine.Execute("print");
        }

        [TestMethod]
        public void Execute_VoidReturningCommand_ReturnsNull()
        {
            Assert.IsTrue(Engine.ExecuteAndCapture("void") == null, "Void command execute did not return null.");
        }
    }
}
