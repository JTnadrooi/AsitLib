using AsitLib.CommandLine;
using AsitLib.CommandLine.Attributes;
using System.Collections.ObjectModel;
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

    public class AlwaysReturnTestFlagHandler : FlagHandler
    {
        public AlwaysReturnTestFlagHandler() : base("ret-test", "desc", "t")
        {

        }

        public override object? OnReturned(FlagContext context, object? returned)
        {
            IReadOnlyList<string> args = context.GetFlagHandlerArguments(this);
            return args.Count > 0 ? args[0] : "TEST";
        }
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

        [CommandAttribute("desc", inheritNamespace: false)]
        public string TvEnable([AllowAntiArgument("disable-color")] bool color = true)
        {
            return "Tv" + (color ? " in color!" : ".");
        }

        [CommandAttribute("desc", inheritNamespace: false, aliases: ["hi"])]
        public string Greet([CustomSignature("name")] string yourName)
        {
            return $"Hi, {yourName}!";
        }

        [CommandAttribute("desc", isMain: true)]
        public void Main()
        {

        }

        [CommandAttribute("desc", inheritNamespace: false)]
        public void Void()
        {

        }

        [CommandAttribute("desc", inheritNamespace: false, id: "impl")]
        public int ImplicitValueAttributeTest([ImplicitValue(1)] int value = 0)
        {
            return value;
        }

        [CommandAttribute("desc", inheritNamespace: false)]
        public string Shorthand([Shorthand("wa")] string wayToLongParameterName, [Shorthand] int secondWayToLongOne = 0)
        {
            return wayToLongParameterName + " | " + secondWayToLongOne;
        }

        [CommandAttribute("desc", inheritNamespace: false)]
        public string FlagConflict([Shorthand("t")] string testAlso)
        {
            return testAlso;
        }
    }

    [TestClass]
    public class CommandEngineTests
    {
        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            Engine = new CommandEngine()
                .RegisterProvider(new TestCommandProvider(), CommandInfoFactory.Default)
                .RegisterFlagHandler(new AlwaysReturnTestFlagHandler());
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
        public void Execute_NoValueCustomAntiArgument_ActsAsFalse()
        {
            AssertExecute("Tv.", "tv-enable --disable-color");
        }

        [TestMethod]
        [ExpectedException(typeof(CommandException))]
        public void Execute_DuplicateArguments_ThrowsError()
        {
            AssertExecute("print.", "print ahoy --input bonjour");
        }

        [TestMethod]
        [ExpectedException(typeof(CommandException))]
        public void Execute_DuplicateAntiArguments_ThrowsError()
        {
            AssertExecute("Tv.", "tv-enable --color false --disable-color");
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
        [ExpectedException(typeof(CommandException))]
        public void Execute_ToManyArguments_ThrowsCommandException()
        {
            Engine.Execute("print hi true doest-exist");
        }

        [TestMethod]
        public void Execute_VoidReturningCommand_ReturnsNull()
        {
            Assert.IsNull(Engine.ExecuteAndCapture("void"), "Void command executing did not return null.");
        }

        [TestMethod]
        public void Execute_WithFlag_HandelsFlag()
        {
            //AssertExecute("TEST", "void -t");
            AssertExecute("TEST", "void --ret-test");
        }

        [TestMethod]
        public void Execute_WithFlagArgument_HandelsFlagArgument()
        {
            AssertExecute("AHOY", "void -t AHOY");
            AssertExecute("AHOY", "void --ret-test AHOY");
        }

        [TestMethod]
        [ExpectedException(typeof(CommandException))]
        public void Execute_DuplicateLongShortOptions_ThrowsCommandException()
        {
            Engine.Execute("shorthand -wa hello --way-to-long-parameter-name hi");
        }

        [TestMethod]
        public void Execute_FlagConflict_PreventsFlag()
        {
            AssertExecute("ahoy", "flag-conflict -t ahoy");
        }

        [TestMethod]
        public void Execute_ImplicitValueAttribute_UsesImplicitValue()
        {
            AssertExecute("0", "impl");
            AssertExecute("1", "impl --value");
            AssertExecute("2", "impl --value 2");
        }

        [TestMethod]
        public void Execute_Namespace_CallsMainCommand()
        {
            Engine.Execute("test");
        }
    }
}
