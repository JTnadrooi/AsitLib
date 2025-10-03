using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;
using System.Linq;

namespace AsitLib.Stele
{
    public static class Stele
    {
        private static readonly string CorePath = $@"C:\Users\{Environment.UserName}\source\repos\STOLON\.ignore\fax-128";
        private static readonly string InPath = CorePath + ".png";
        private static readonly string OutPath = CorePath + "." + FileExtension;

        private static readonly int Version = 0;
        private static readonly string FileExtension = "stele";

        public static void Run() // debug
        {
            using Image<Rgba32> img = Image.Load<Rgba32>(InPath);

            Rgba32[] data = new Rgba32[img.Width * img.Height];
            img.CopyPixelDataTo(data);

            for (int i = 0; i < 10; i++)
            {
                var pixel = data[i];
                Console.WriteLine($"Pixel {i}: {pixel.R}, {pixel.G}, {pixel.B}, {pixel.A}");
            }
            Console.WriteLine($"OG filesize: {new FileInfo(InPath).Length}.");

            if (!File.Exists(OutPath)) File.Create(OutPath).Dispose();
            else File.WriteAllBytes(OutPath, Array.Empty<byte>());

            Encode(OutPath, data, img.Width, img.Height);

            Console.WriteLine($"NEW filesize: {new FileInfo(OutPath).Length}.");

            Console.WriteLine($"{Convert.ToString(0b0000_0001 << 3, toBase: 2)}");
        }

        public static void Encode(string path, Rgba32[] data, int width, int height, bool useRLE = false)
        {
            if (width % 4 != 0 || width < 4) throw new ArgumentException(nameof(width));
            if (height % 4 != 0 || height < 4) throw new ArgumentException(nameof(height));

            using FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Write);
            using BinaryWriter writer = new BinaryWriter(fs);

            const int R1 = 23;
            const int R2 = 242;

            // header.

            writer.Write((byte)Version);
            writer.Write((ushort)(width / 4 - 4));
            writer.Write((ushort)(height / 4 - 4));

            // body.

            if (useRLE)
            {

            }
            else
            {
                byte buffer = 0b_0000_0000;

                byte GetPixelOverlay(ref Rgba32 pixel, int index) // (0 = color_0, 1 = color_1, 2 = transparent)
                {
                    return (byte)((pixel.R switch
                    {
                        R1 => (byte)0,
                        R2 => (byte)1,
                        _ => throw new Exception(),
                    }) << (index * 2));
                }

                for (int i = 3; i < data.Length; i += 4)
                {
                    buffer = (byte)(buffer | GetPixelOverlay(ref data[i], 0));
                    buffer = (byte)(buffer | GetPixelOverlay(ref data[i - 1], 1));
                    buffer = (byte)(buffer | GetPixelOverlay(ref data[i - 2], 2));
                    buffer = (byte)(buffer | GetPixelOverlay(ref data[i - 3], 3));

                    writer.Write(buffer);
                }
            }
        }
    }
}
