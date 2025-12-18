namespace AsitLib.Tests
{
    public class TestCommandGroup : CommandGroup
    {
        public TestCommandGroup() : base("testg", CommandInfoFactory.Default)
        //public TestCommandGroup() : base("testg", nameof(Main))
        {

        }

        //[Command("desc")]
        //public void Main()
        //{

        //}

        [Command("desc")]
        public string Print(string msg) => msg;
    }

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
        public void Execute_SubCommand_HasCorrectId()
        {
            AssertExecute("bonjour", "testg print bonjour");
        }
    }
}
