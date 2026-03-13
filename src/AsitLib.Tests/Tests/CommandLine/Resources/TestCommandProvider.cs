using System.ComponentModel.DataAnnotations;

namespace AsitLib.Tests
{
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

        [Command("desc", Aliases = ["o", "basic"])]
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
    }
}
