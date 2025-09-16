using System;
using AsitLib.Debug;

namespace AsitLib.ConsoleTests
{
    class Program
    {
        static void Main(string[] args)
        {
            DebugStream debug = new DebugStream(header: "test");

            debug.Log(">[s]initializing system.");
            debug.Log("by nadrooi.");
            debug.Log(">[s]loading config.");
            debug.Log(">connecting to DB.");
            debug.Log("a");
            debug.Log("<connection successful.", new object?[] { "Server=127.0.0.1", null });
            debug.Success();
            debug.Warn("missing optional feature.");
            debug.Success();
            Console.Read();
        }
    }
}
