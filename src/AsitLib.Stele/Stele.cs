using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.Memory;
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
        private static readonly string OutPath2 = CorePath + "2." + FILE_EXTENSION;

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

                Console.WriteLine($"og(png) filesize: {new FileInfo(InPath).Length}.");

                if (!File.Exists(OutPath)) File.Create(OutPath).Dispose();
                else File.WriteAllBytes(OutPath, Array.Empty<byte>());

                float swNoRLEMs = Test(() => Encode(OutPath, data, img.Width, img.Height, false), "new(stele, NO_RLE)", 100);

                File.WriteAllBytes(OutPath, Array.Empty<byte>());

                float swRLEMs = Test(() => Encode(OutPath, data, img.Width, img.Height, true), "new(stele, RLE)", 100);

                Rgba32[] outData = new Rgba32[img.Width * img.Height];
                float swDecodeMs = Test(() => Decode(OutPath, outData, new Rgba32(23, 18, 25), new Rgba32(242, 251, 235)), "dec(stele)", 1000, false);
                Console.WriteLine($"\tpassed: " + Enumerable.SequenceEqual(data, outData));
                Console.WriteLine($"\tspeed increase: ~" + Math.Round(pngDec.ElapsedMilliseconds / swDecodeMs) + "x");
            }
        }



        public static void Encode(string path, Rgba32[] data, int width, int height, bool useRLE) // missing transparent rle
        {
            if (width % 4 != 0 || width < 4) throw new ArgumentException(nameof(width));
            if (height % 4 != 0 || height < 4) throw new ArgumentException(nameof(height));

            //using FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Write);
            using FileStream fs = File.Create(path);
            using BinaryWriter writer = new BinaryWriter(fs);

            const int R1 = 23;
            const int R2 = 242;

            // header.

            writer.Write((byte)VERSION);
            writer.Write((ushort)(width));
            writer.Write((ushort)(height));

            // body.

            const byte REPEAT_OVERLAY = 0b_1100_0000;
            const byte REPEAT_C1 = 0b_0000_0000;
            const byte REPEAT_C2 = 0b_0101_0101;
            const byte INVALID = 0b1111_1111; // bytes like this are impossible for the algoritm to create as it would require the REPEAT flag to exist in other places that the last 2 bits.

            byte GetColorValue(ref Rgba32 pixel) => pixel.R switch
            {
                _ when pixel.A == 0 => (byte)2,
                R1 => (byte)0,
                R2 => (byte)1,
                _ => throw new Exception(),
            };
            byte GetPixelOverlay(ref Rgba32 pixel, int index) => GetValueOverlay(GetColorValue(ref pixel), index);
            byte GetValueOverlay(in byte value, int index) => (byte)(value << (index * 2));
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

            byte buffer = 0b_0000_0000;

            if (useRLE)
            {
                int repeatCount = 0;
                byte pendingPixelBuffer = INVALID;

                int repeatEntitled = -1;

                for (int i = 3; i < data.Length; i += 4)
                {
                    buffer = (byte)(buffer | GetPixelOverlay(ref data[i], 3));
                    buffer = (byte)(buffer | GetPixelOverlay(ref data[i - 1], 2));
                    buffer = (byte)(buffer | GetPixelOverlay(ref data[i - 2], 1));
                    buffer = (byte)(buffer | GetPixelOverlay(ref data[i - 3], 0));

                    switch (repeatEntitled)
                    {
                        case 0:
                        case 1:
                            if (buffer == GetRepeatByte(repeatEntitled) && repeatCount < byte.MaxValue)
                            {
                                repeatCount++;
                                break;
                            }
                            else goto case -1;
                        case -1:
                            if (repeatCount > 0)
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
                    buffer = (byte)(buffer | GetPixelOverlay(ref data[i], 3));
                    buffer = (byte)(buffer | GetPixelOverlay(ref data[i - 1], 2));
                    buffer = (byte)(buffer | GetPixelOverlay(ref data[i - 2], 1));
                    buffer = (byte)(buffer | GetPixelOverlay(ref data[i - 3], 0));

                    writer.Write(buffer);
                    buffer = 0b_0000_0000;
                }
            }

            writer.Flush();
            fs.Flush();
        }
        public static void Decode<T>(string path, T[] outData, T value1, T value2, int bufferLength = 16384) where T : struct
        {
            using FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            using BinaryReader reader = new BinaryReader(fs);

            byte version = reader.ReadByte();
            ushort width = reader.ReadUInt16();
            ushort height = reader.ReadUInt16();

            if (outData.Length != width * height) throw new ArgumentException(nameof(outData));

            byte[] largeBuffer = new byte[Math.Min(outData.Length / 4, bufferLength)];
            int outIndex = 0;
            int bytesRead;
            int runLength;

            T[] map = [value1, value2];

            while ((bytesRead = reader.Read(largeBuffer, 0, largeBuffer.Length)) > 0)
            {
                for (int bufferIndex = 0; bufferIndex < bytesRead; bufferIndex++)
                {
                    byte buffer = largeBuffer[bufferIndex];

                    for (int i = 0; i < 4; i++)
                    {
                        int halfNib = (buffer >> (i * 2)) & 0b_0000_0011;

                        if (halfNib == 3) // RLE marker (last 2 bits)
                        {
                            T lastValue = outData[outIndex - 1]; // get last written value.

                            runLength = ((bufferIndex < bytesRead - 1) ? largeBuffer[bufferIndex + 1] : reader.ReadByte()) * 4 + 1;

                            bufferIndex++;
                            Array.Fill(outData, lastValue, outIndex, runLength);

                            outIndex += runLength;
                            break;
                        }
                        else
                        {
                            outData[outIndex] = map[halfNib];
                            outIndex++;
                        }
                    }
                }
            }

            if (outIndex != outData.Length) throw new Exception("Output data size mismatch.");
        }
    }
}
