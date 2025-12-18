namespace AsitLib.Tests
{
    [TestClass]
    public static class TestHelpers
    {
        public const string TEST_DIRECTORY = "AsitLibTests__TESTS\\";

        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            DirectoryInfo di = Directory.CreateDirectory(TEST_DIRECTORY);
            foreach (FileInfo file in di.GetFiles()) file.Delete();
            foreach (DirectoryInfo dir in di.GetDirectories()) dir.Delete(true);
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {

        }
    }
}
