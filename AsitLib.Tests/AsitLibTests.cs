using AsitLib.CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib.Tests
{
    public class TestCommandProvider : CommandProvider
    {
        public TestCommandProvider() : base("test") { }

        [Command("desc", inheritNamespace: false)]
        public void Print(string input, string? input2)
        {
            Console.WriteLine(input + " " + input);
        }
    }

    public static class AsitLibTestExtensions
    {
        public static void AssertExecute(string expected, string args) => AssertExecute(expected, ParseHelpers.Split(args));
        public static void AssertExecute(string expected, string[] args)
        {
            Assert.AreEqual(expected, AsitLibTests.Engine.ExecuteAndCapture(args));
        }
    }

    [TestClass]
    public class AsitLibTests
    {
        [AssemblyInitialize]
        public static void AssemblyInit()
        {
            Engine = new CommandEngine<CommandAttribute, CommandInfo>(CommandInfoFactory.Default)
                .RegisterProvider(new TestCommandProvider())
                .Initialize();

            Engine.Execute("print sus --input2 bonjour");
        }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public static CommandEngine<CommandAttribute, CommandInfo> Engine { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    }
}
