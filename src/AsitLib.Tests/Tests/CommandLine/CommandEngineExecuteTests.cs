using System.Diagnostics.CodeAnalysis;

namespace AsitLib.Tests
{
    [TestClass]
    public class CommandEngineExecuteTests
    {
        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            Engine = new CommandEngine()
                .AddProvider(new TestCommandProvider())
                .AddGlobalOption(new AlwaysReturnTestGlobalOptionHandler());
        }

        [NotNull]
        public static CommandEngine? Engine { get; private set; }

        public static void AssertExecute(string expected, string args) => AssertExecute(expected, ParseHelpers.SplitWithRespectForQuotes(args));
        public static void AssertExecute(string expected, string[] args)
        {
            Engine.Execute(args).ToOutputString().Should().Be(expected);
        }

        public static void AssertCheckOutput()
        {
            Assert.Inconclusive("Check output.");
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
            AssertExecute("Tv.", "tv --disable-color");
        }

        [TestMethod]
        public void Execute_DuplicateArguments_ThrowsEx()
        {
            Invoking(() => AssertExecute("print.", "print ahoy --input bonjour")).Should().Throw<CommandException>();
        }

        [TestMethod]
        public void Execute_DuplicateAntiArguments_ThrowsEx()
        {
            Invoking(() => AssertExecute("Tv.", "tv --color false --disable-color")).Should().Throw<CommandException>();
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
        public void Execute_InvalidParameter_ThrowsCommandException()
        {
            Invoking(() => Engine.Execute("print hi --doesnt-exist ahoy")).Should().Throw<CommandException>();
        }

        [TestMethod]
        public void Execute_MissingArgument_ThrowsCommandException()
        {
            Invoking(() => Engine.Execute("print"));
        }

        [TestMethod]
        public void Execute_ToManyArguments_ThrowsCommandException()
        {
            Invoking(() => Engine.Execute("print hi true doest-exist")).Should().Throw<CommandException>();
        }

        [TestMethod]
        public void Execute_VoidReturningCommand_IsVoid()
        {
            Assert.IsTrue(Engine.Execute("void").IsVoid);
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
        public void Execute_DuplicateLongShortOptions_ThrowsCommandException()
        {
            Invoking(() => Engine.Execute("shorthand -wa hello --way-to-long-parameter-name hi")).Should().Throw<CommandException>();
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
        public void Execute_ArrayReturningCommand_PrintsValuesOnNewLine()
        {
            AssertExecute("1\n2\n3\n4", "array");
        }

        [TestMethod]
        public void Execute_DictionaryReturningCommand_PrintsKeyValuePairsOnNewLine()
        {
            AssertExecute("10=1\n20=2\n30=3", "dictionary");
        }

        [TestMethod]
        public void Execute_WithDataAnnotatedInvalidCommand_ThrowsEx()
        {
            Invoking(() => Engine.Execute("validation 11")).Should().Throw<CommandException>();
        }

        [TestMethod]
        public void Execute_HelpFullStopGlobalOption()
        {
            Console.WriteLine(Engine.Execute("void --help"));
            Console.WriteLine("\n" + Engine.Execute("void -h"));
            AssertCheckOutput();
        }
    }
}
