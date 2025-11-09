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

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public static CommandEngine<CommandAttribute, CommandInfo> Engine { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    }

    //public static class ConsoleAcces
    //{

    //    //public static void Run()
    //    //{
    //    //    AsitLibTests.AssemblyInit(null!);

    //    //    Console.WriteLine(AsitLibTests.Engine.UniqueCommands.ToJoinedString(", "));
    //    //    Console.WriteLine(AsitLibTests.Engine.Commands.ToJoinedString(", "));
    //    //}
    //}
}
