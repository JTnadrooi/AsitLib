using AsitLib.CommandLine;
using FluentAssertions;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace AsitLib.Tests
{
    public enum TestEnum
    {
        Value0 = 0,
        Value1 = 1,
        Value2 = 2,
        [Signature("three")]
        Value3 = 3,
    }

    public class AlwaysReturnTestGlobalOptionHandler : GlobalOption
    {
        public AlwaysReturnTestGlobalOptionHandler() : base("ret-test", "desc", "t", new OptionInfo(typeof(string))
        {
            ImplicitValue = "TEST",
        })
        {

        }

        public override object? OnReturned(CommandContext context, object? returned)
        {
            if (context.TryGetGlobalOptionValue<string>(this, out string? value)) return value;
            else return returned;
        }
    }

    public class TestCommandProvider : CommandProvider
    {
        public TestCommandProvider() : base("test", CommandInfoFactory.Default) { }

        [Command("desc")]
        public string Print(string input, bool upperCase = false)
        {
            return upperCase ? input.ToUpper() : input;
        }

        [Command("desc")]
        public string Tv([Option(AntiParameterName = "no-color")] bool color = true)
        {
            return "Tv" + (color ? " in color!" : ".");
        }

        [Command("desc")]
        public string TvEnable([Option(AntiParameterName = "disable-color")] bool color = true)
        {
            return "Tv" + (color ? " in color!" : ".");
        }

        [Command("desc", Aliases = ["hi"])]
        public string Greet([Option(Name = "name")] string yourName)
        {
            return $"Hi, {yourName}!";
        }

        [Command("desc", IsGenericFlag = true, Aliases = ["o", "basic"])]
        public void Void()
        {

        }

        [Command("desc", Id = "impl")]
        public int ImplicitValueAttributeTest([Option(ImplicitValue = 1)] int value = 0)
        {
            return value;
        }

        [Command("desc")]
        public string Shorthand([Option(Shorthand = "wa")] string wayToLongParameterName, [Option(Shorthand = "s")] int secondWayToLongOne = 0)
        {
            return wayToLongParameterName + " | " + secondWayToLongOne;
        }

        [Command("desc")]
        public string FlagConflict([Option(Shorthand = "t")] string testAlso)
        {
            return testAlso;
        }

        [Command("desc")]
        public int[] Array()
        {
            return [1, 2, 3, 4];
        }

        [Command("desc")]
        public Dictionary<int, int> Dictionary()
        {
            return new Dictionary<int, int> {
                { 10, 1 },
                { 20, 2 },
                { 30, 3 },
            };
        }

        [Command("desc")]
        public void Validation([Range(0, 10)] int i)
        {

        }

        [Command("desc")]
        public string ContextInject(CommandContext context)
        {
            return context.Engine.Providers["test"].Name;
        }
    }

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

        [ClassCleanup]
        public static void ClassCleanup()
        {

        }

        [NotNull]
        public static CommandEngine? Engine { get; private set; }

        public static void AssertExecute(string expected, string args) => AssertExecute(expected, ParseHelpers.Split(args));
        public static void AssertExecute(string expected, string[] args)
        {
            Assert.AreEqual(expected, Engine.Execute(args).ToOutputString());
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
        }

        [TestMethod]
        public void Execute_NoValueCustomAntiArgument_ActsAsFalse()
        {
            AssertExecute("Tv.", "tv-enable --disable-color");
        }

        [TestMethod]
        public void Execute_DuplicateArguments_ThrowsError()
        {
            Assert.Throws<CommandException>(() => AssertExecute("print.", "print ahoy --input bonjour"));
        }

        [TestMethod]
        public void Execute_DuplicateAntiArguments_ThrowsError()
        {
            Assert.Throws<CommandException>(() => AssertExecute("Tv.", "tv-enable --color false --disable-color"));
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
            Assert.Throws<CommandException>(() => Engine.Execute("print hi --doesnt-exist ahoy"));
        }

        [TestMethod]
        public void Execute_MissingArgument_ThrowsCommandException()
        {
            Assert.Throws<CommandException>(() => Engine.Execute("print"));
        }

        [TestMethod]
        public void Execute_ToManyArguments_ThrowsCommandException()
        {
            Assert.Throws<CommandException>(() => Engine.Execute("print hi true doest-exist"));
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
            Assert.Throws<CommandException>(() => Engine.Execute("shorthand -wa hello --way-to-long-parameter-name hi"));
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
        public void Execute_WithDataAnnotatedInvalidCommand_ThrowsError()
        {
            Assert.Throws<CommandException>(() => Engine.Execute("validation 11"));
        }

        [TestMethod]
        public void Execute_GenericFlag()
        {
            Engine.Execute("void");
            Engine.Execute("--void");
            Engine.Execute("--basic");
            Engine.Execute("-o");
        }

        [TestMethod]
        public void Execute_InvalidGenericFlag_ThrowsEx()
        {
            Engine.Execute("void");
            Engine.Execute("--void");
            Engine.Execute("--basic");
            Engine.Execute("-o");
        }

        [TestMethod]
        public void Execute_HelpFullStopGlobalOption()
        {
            Console.WriteLine(Engine.Execute("void --help"));
            Console.WriteLine("\n" + Engine.Execute("void -h"));
            AssertCheckOutput();
        }

        [TestMethod]
        public void Execute_ContextOption_InjectsContext()
        {
            AssertExecute("test", "context-inject");
        }
    }
}
