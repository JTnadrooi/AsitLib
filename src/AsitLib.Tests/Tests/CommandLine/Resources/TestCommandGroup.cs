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
}
