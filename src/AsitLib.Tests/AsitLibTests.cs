using AsitLib.CommandLine;

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
    }

    [TestClass]
    public class AsitLibTests
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            Engine = new CommandEngine<CommandAttribute, CommandInfo>(CommandInfoFactory.Default)
                .RegisterProvider(new TestCommandProvider())
                .Initialize();
        }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public static CommandEngine<CommandAttribute, CommandInfo> Engine { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    }
}
