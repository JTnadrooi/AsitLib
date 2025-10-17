using AsitLib.Debug;
using AsitLib.Stele;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AsitLib.ConsoleTests
{
    class Program
    {
        public static void Main(string[] args)
        {
            SteleTests.Run();

            Console.Read();
        }

        public static void LogShowCase()
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

        public static void Run() // debug
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
