using AsitLib.CommandLine;

namespace AsitLib.Tests
{
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

        public static void Run()
        {
            AssemblyInit(null!);

            Console.WriteLine(Engine.UniqueCommands.ToJoinedString(", "));
            Console.WriteLine(Engine.Commands.ToJoinedString(", "));
        }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public static CommandEngine<CommandAttribute, CommandInfo> Engine { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    }
}
