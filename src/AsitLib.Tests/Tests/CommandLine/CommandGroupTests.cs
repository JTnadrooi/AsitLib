namespace AsitLib.Tests
{

    [TestClass]
    public class CommandGroupTests
    {
        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            Engine = new CommandEngine()
                .AddProvider(new TestCommandGroup());
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {

        }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public static CommandEngine Engine { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        public static void AssertExecute(string expected, string args) => Engine.Execute(args).ToOutputString().Should().Be(expected);
        //public static void AssertExecute(string expected, string[] args)
        //{
        //    Engine.Execute(args).ToOutputString().Should().Be(expected);
        //}

        [TestMethod]
        public void Execute_ChildCommandWithPositionalArgument()
        {
            AssertExecute("bonjour", "testg print bonjour");
        }

        [TestMethod]
        public void Execute_ChildCommand_HasCorrectId()
        {
            Engine.Commands.Should().ContainKey("testg print");
        }
    }
}
