using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace AsitLib.Stele
{
    public static class Stele
    {
        private static readonly string CorePath = $@"C:\Users\{Environment.UserName}\source\repos\STOLON\.ignore\silo-512";
        //private static readonly string CorePath = $@"C:\Users\{Environment.UserName}\source\repos\STOLON\.ignore\fax-128";

        private const string FILE_EXTENSION = "stele";
        private const int VERSION = 0;

        private static readonly string InPath = CorePath + ".png";
        private static readonly string OutPath = CorePath + "." + FILE_EXTENSION;

        private static string B(byte b)
        {
            string binary = Convert.ToString(b, 2).PadLeft(8, '0');
            return binary.Substring(0, 4) + "_" + binary.Substring(4);
        }

        public static void Run() // debug
        {
            using Image<Rgba32> img = Image.Load<Rgba32>(InPath);

            Rgba32[] data = new Rgba32[img.Width * img.Height];
            img.CopyPixelDataTo(data);

            //for (int i = 0; i < 10; i++)
            //{
            //    var pixel = data[i];
            //    Console.WriteLine($"Pixel {i}: {pixel.R}, {pixel.G}, {pixel.B}, {pixel.A}");
            //}
            Console.WriteLine($"og(png) filesize: {new FileInfo(InPath).Length}.");

            // start

            if (!File.Exists(OutPath)) File.Create(OutPath).Dispose();
            else File.WriteAllBytes(OutPath, Array.Empty<byte>());

            Stopwatch s = Stopwatch.StartNew();

            Encode(OutPath, data, img.Width, img.Height, false);

            s.Stop();

            Console.WriteLine($"new(stele, NO_RLE) filesize: {new FileInfo(OutPath).Length}, time taken: ~" + s.ElapsedMilliseconds + "ms");

            if (!File.Exists(OutPath)) File.Create(OutPath).Dispose();
            else File.WriteAllBytes(OutPath, Array.Empty<byte>());

            Stopwatch s2 = Stopwatch.StartNew();

            Encode(OutPath, data, img.Width, img.Height, true);

            s2.Stop();

            Console.WriteLine($"new(stele, RLE) filesize: {new FileInfo(OutPath).Length}, time taken: ~" + s2.ElapsedMilliseconds + "ms");

            //Console.WriteLine($"{Convert.ToString(0b0000_0001 << 3, toBase: 2)}");
        }

        public static void Encode(string path, Rgba32[] data, int width, int height, bool useRLE)
        {
            if (width % 4 != 0 || width < 4) throw new ArgumentException(nameof(width));
            if (height % 4 != 0 || height < 4) throw new ArgumentException(nameof(height));

            using FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Write);
            using BinaryWriter writer = new BinaryWriter(fs);

            const int R1 = 23;
            const int R2 = 242;

            // header.

            writer.Write((byte)VERSION);
            writer.Write((ushort)(width / 4 - 4));
            writer.Write((ushort)(height / 4 - 4));

            // body.

            const byte REPEAT_OVERLAY = 0b_1100_0000;
            const byte REPEAT_C1 = 0b_0000_0000;
            const byte REPEAT_C2 = 0b_0101_0101;
            const byte INVALID = 0b1111_1111; // bytes like this are impossible for the algoritm to create as it would require the REPEAT flag to exist in other places that the last 2 bits.

            //byte GetPixelOverlay(ref Rgba32 pixel, int index) => (byte)((pixel.A == 0 ? (byte)3 : (pixel.R switch
            //{
            //    R1 => (byte)0,
            //    R2 => (byte)1,
            //    _ => throw new Exception(),
            //})) << (index * 2));
            byte GetColorValue(ref Rgba32 pixel) => pixel.R switch
            {
                _ when pixel.A == 0 => (byte)2,
                R1 => (byte)0,
                R2 => (byte)1,
                _ => throw new Exception(),
            };
            byte GetPixelOverlay(ref Rgba32 pixel, int index) => GetValueOverlay(GetColorValue(ref pixel), index);
            byte GetValueOverlay(in byte value, int index) => (byte)(value << index * 2);
            byte GetRepeatByte(in int color) => color switch
            {
                0 => REPEAT_C1,
                1 => REPEAT_C2,
                _ => throw new Exception(),
            };
            int IsRepeatEntitled(byte buffer) => (byte)(buffer & 0b1111_0000) switch
            {
                0b0101_0000 => 1,
                0b0000_0000 => 0,
                _ => -1
            };

            //Console.WriteLine("a " + IsRepeatEntitled(0b_0000_0000));
            //Console.WriteLine("a " + IsRepeatEntitled(0b_0100_0001));
            //Console.WriteLine("a " + IsRepeatEntitled(0b_1000_0011));

            //Console.WriteLine("a " + B(GetValueOverlay(1, 1)));

            byte buffer = 0b_0000_0000;

            if (useRLE)
            {
                int repeatCount = 0;
                byte pendingPixelBuffer = INVALID;

                int repeatEntitled = -1;

                for (int i = 3; i < data.Length; i += 4)
                {
                    buffer = (byte)(buffer | GetPixelOverlay(ref data[i], 0));
                    buffer = (byte)(buffer | GetPixelOverlay(ref data[i - 1], 1));
                    buffer = (byte)(buffer | GetPixelOverlay(ref data[i - 2], 2));
                    buffer = (byte)(buffer | GetPixelOverlay(ref data[i - 3], 3));

                    switch (repeatEntitled)
                    {
                        case 0:
                            if (buffer == REPEAT_C1)
                            {
                                repeatCount++;
                                break;
                            }
                            else
                            {
                                goto case -1;
                            }
                        case 1:
                            if (buffer == REPEAT_C2)
                            {
                                repeatCount++;
                                break;
                            }
                            else
                            {
                                goto case -1;
                            }
                        case -1:
                            if (repeatCount > 1)
                            {
                                writer.Write((byte)(pendingPixelBuffer | REPEAT_OVERLAY));
                                writer.Write((byte)repeatCount);
                            }
                            else if (pendingPixelBuffer != INVALID) writer.Write(pendingPixelBuffer);

                            repeatCount = 0;
                            repeatEntitled = IsRepeatEntitled(buffer);
                            pendingPixelBuffer = buffer;

                            break;
                    }

                    buffer = 0b_0000_0000;
                }

                if (repeatCount > 1)
                {
                    writer.Write((byte)(pendingPixelBuffer | REPEAT_OVERLAY));
                    writer.Write((byte)repeatCount);
                }
                else if (pendingPixelBuffer != INVALID) writer.Write(pendingPixelBuffer);
            }
            else
            {
                for (int i = 3; i < data.Length; i += 4)
                {
                    buffer = (byte)(buffer | GetPixelOverlay(ref data[i], 0));
                    buffer = (byte)(buffer | GetPixelOverlay(ref data[i - 1], 1));
                    buffer = (byte)(buffer | GetPixelOverlay(ref data[i - 2], 2));
                    buffer = (byte)(buffer | GetPixelOverlay(ref data[i - 3], 3));

                    writer.Write(buffer);
                    buffer = 0b_0000_0000;
                }
            }

            writer.Flush();
            fs.Flush();
        }
    }
}
