using AsitLib.CommandLine;
using AsitLib.Debug;
using AsitLib.Stele;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace AsitLib.ConsoleTests
{
    class Program
    {
        public static void Main(string[] args)
        {
            //LogShowCase();
            CLITests.Run();

            Console.Read();
        }

        public static void LogShowCase()
        {
            Logger logger = new Logger(header: "test");

            logger.Log(">[s]initializing system.");
            logger.Log("by nadrooi.");
            logger.Log(">loading config.");
            //Thread.Sleep(40);
            logger.Log(">connecting to DB.");
            //Thread.Sleep(100);
            logger.Log("a");
            logger.Log("<connection successful.", new object?[] { "Server=127.0.0.1", null });
            logger.Success();
            logger.Warn("missing optional feature.");
            logger.Success();
        }
    }

    public static class CLITests
    {
        public class TestCommandProvider : CommandProvider
        {
            public TestCommandProvider() : base("test") { }

            [Command("desc", inheritNamespace: false)]
            public void Print(string input, string input2)
            {
                Console.WriteLine(input + " " + input2);
            }
        }

        public static void Run()
        {
            CommandEngine<CommandAttribute, CommandInfo> engine = new CommandEngine<CommandAttribute, CommandInfo>(CommandInfoFactory.Default)
                .RegisterProvider(new TestCommandProvider())
                .Initialize();

            //engine.Execute(["cmd", "--b", "yay", "AYO", "--", "--", "--a"]);
            engine.Execute("print --input bonjour");
        }
    }

    public static class SteleTests
    {
        private static readonly string CorePath = $@"C:\Users\{Environment.UserName}\source\repos\STOLON\.ignore\silo-512";
        //private static readonly string CorePath = $@"C:\Users\{Environment.UserName}\source\repos\STOLON\.ignore\fax-128";

        private static readonly string InPath = CorePath + ".png";
        private static readonly string OutPath = CorePath + "." + SteleData.FILE_EXTENSION;
        private static readonly string OutPath2 = CorePath + "2." + SteleData.FILE_EXTENSION;

        private static string B(byte b)
        {
            string binary = Convert.ToString(b, 2).PadLeft(8, '0');
            return binary.Substring(0, 4) + "_" + binary.Substring(4);
        }

        public static void Run()
        {
            float Test(Action action, string id, int count, bool showFileSize = true)
            {
                Stopwatch sw = Stopwatch.StartNew();
                for (int i = 0; i < count; i++) action();
                sw.Stop();

                long fileSize = new FileInfo(OutPath).Length;
                Console.WriteLine($"{id} time taken: ~" + (sw.ElapsedMilliseconds / (float)count) + $"ms{(showFileSize ? $", filesize: {fileSize} bytes" : string.Empty)}");
                return (sw.ElapsedMilliseconds / (float)count);
            }

            Stopwatch pngDec = Stopwatch.StartNew();
            using (Image<Rgba32> img = Image.Load<Rgba32>(InPath))
            {
                pngDec.Stop();
                Console.WriteLine($"dec(png) time taken: ~" + pngDec.ElapsedMilliseconds + "ms");

                Rgba32[] data = new Rgba32[img.Width * img.Height];
                img.CopyPixelDataTo(data);
                //SteleMap<Rgba32> map = SteleMap<Rgba32>.CreateFromUnique([new Rgba32(23, 18, 25), new Rgba32(242, 251, 235)]);
                SteleMap<Rgba32> map = SteleMap<Rgba32>.Create(data);
                Console.WriteLine($"pixelcount: {data.Length}.");

                Console.WriteLine($"og(png) filesize: {new FileInfo(InPath).Length}.");

                if (!File.Exists(OutPath)) File.Create(OutPath).Dispose();
                else File.WriteAllBytes(OutPath, Array.Empty<byte>());

                float swRLEMs = Test(() => SteleData<Rgba32>.Encode(OutPath, data, img.Width, img.Height, map), "new(stele, RLE)", 100);

                Rgba32[] outData = new Rgba32[img.Width * img.Height];
                float swDecodeMs = Test(() => SteleData<Rgba32>.GetData(OutPath, outData, map), "dec(stele)", 1000, false);
                Console.WriteLine($"\tpassed: " + Enumerable.SequenceEqual(data, outData));
                Console.WriteLine($"\tspeed increase: ~" + Math.Round(pngDec.ElapsedMilliseconds / swDecodeMs) + "x");
            }
        }
    }
}
