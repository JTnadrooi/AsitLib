using AsitLib.CommandLine;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib.Tests.Tests.CommandLine
{
    [TestClass]
    public class CommandEngineTests
    {
        [TestInitialize]
        public void TestInit()
        {
            Engine = new CommandEngine();
        }

        [TestCleanup]
        public void TestCleanup()
        {

        }

        [NotNull]
        public static CommandEngine? Engine { get; private set; }

        public static DummyCommandProvider OneCommandCommandProvider { get; }

        static CommandEngineTests()
        {
            OneCommandCommandProvider = new DummyCommandProvider("test", commands: []);

            OneCommandCommandProvider.Commands.Add(new DummyCommandInfo("test-command") { Provider = OneCommandCommandProvider });

        }

        [TestMethod]
        public void GetProviderCommands_FromTestProvider()
        {
            Engine.AddProvider(new TestCommandProvider());
            Assert.AreEqual(Engine.GetProviderCommands("test").First().Provider!.Name, "test");
        }

        [TestMethod]
        public void AddProvider_AddsProvider()
        {
            Engine.AddProvider(OneCommandCommandProvider);

            Assert.AreEqual(Engine.Providers.Single().Value.Name, OneCommandCommandProvider.Name, $"Only provider added is not the {nameof(OneCommandCommandProvider)}.");
            Assert.IsTrue(Engine.Commands.ContainsKey(OneCommandCommandProvider.GetCommands()[0].Id), $"Command is not added.");
        }
    }
}
