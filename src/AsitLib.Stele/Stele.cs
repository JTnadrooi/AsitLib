using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace AsitLib.Stele
{
    public class SteleMap<T> where T : struct
    {
        public FrozenDictionary<T, int> Map { get; }
        public ReadOnlyCollection<T> InverseMap { get; }
        public int Count => Map.Count;
        public T this[int i] => InverseMap[i];
        public int this[T value] => Map[value];

        private SteleMap(FrozenDictionary<T, int> source)
        {
            Map = source;

            InverseMap = Map.OrderBy(x => x.Value)
                .Select(x => x.Key)
                .ToList().AsReadOnly();
        }

        public static SteleMap<T> Create(IEnumerable<T> values) => CreateFromUnique(values.Distinct());
        public static SteleMap<T> CreateFromUnique(IEnumerable<T> values)
        {
            int count = values.Count();
            if (count > 3 || count < 2) throw new ArgumentException("Invalid source array size.", nameof(values));
            return new SteleMap<T>(values.Select((v, i) => new KeyValuePair<T, int>(v, i)).ToFrozenDictionary());
        }
    }

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
                var map = SteleMap<Rgba32>.CreateFromUnique([new Rgba32(23, 18, 25), new Rgba32(242, 251, 235)]);
                Console.WriteLine($"pixelcount: {data.Length}.");

                Console.WriteLine($"og(png) filesize: {new FileInfo(InPath).Length}.");

                if (!File.Exists(OutPath)) File.Create(OutPath).Dispose();
                else File.WriteAllBytes(OutPath, Array.Empty<byte>());

                float swRLEMs = Test(() => Encode(OutPath, data, img.Width, img.Height, map), "new(stele, RLE)", 100);

                Rgba32[] outData = new Rgba32[img.Width * img.Height];
                float swDecodeMs = Test(() => Decode(OutPath, outData, map), "dec(stele)", 1000, false);
                Console.WriteLine($"\tpassed: " + Enumerable.SequenceEqual(data, outData));
                Console.WriteLine($"\tspeed increase: ~" + Math.Round(pngDec.ElapsedMilliseconds / swDecodeMs) + "x");
            }
        }

        public static void Encode<T>(string path, T[] data, int width, int height, SteleMap<T> map, int bufferLength = 16384) where T : struct
        {
            if ((width % 4) != 0 || width < 4) throw new ArgumentException(nameof(width));
            if ((height % 4) != 0 || height < 4) throw new ArgumentException(nameof(height));

            using FileStream fs = File.Create(path, bufferLength, FileOptions.None);
            using BinaryWriter writer = new BinaryWriter(fs);

            writer.Write((byte)VERSION);
            writer.Write((ushort)width);
            writer.Write((ushort)height);

            const byte REPEAT_OVERLAY = 0b_1100_0000;
            const byte INVALID = 0b_1111_1111;

            byte prev = INVALID;
            int repeatCount = 0;
            int repeatEntitled = -1;
            byte current;

            for (int i = 3; i < data.Length; i += 4)
            {
                current = (byte)((map[data[i - 3]] << 0)
                          | (map[data[i - 2]] << 2)
                          | (map[data[i - 1]] << 4)
                          | (map[data[i]] << 6));

                switch (repeatEntitled)
                {
                    case 0 or 1:
                        if (current == (repeatEntitled switch
                        {
                            0 => 0b_0000_0000,
                            1 => 0b_0101_0101,
                            3 => 0b_1010_1010,
                            _ => INVALID
                        }) && repeatCount < byte.MaxValue)
                        {
                            repeatCount++;
                            continue;
                        }
                        goto default;
                    default:
                        if (repeatCount > 0)
                        {
                            writer.Write((byte)(prev | REPEAT_OVERLAY));
                            writer.Write((byte)repeatCount);
                        }
                        else if (prev != INVALID) writer.Write((byte)prev);

                        repeatCount = 0;
                        repeatEntitled = (current & 0b1111_0000) switch
                        {
                            0b_1010_0000 => 2,
                            0b_0101_0000 => 1,
                            0b_0000_0000 => 0,
                            _ => -1
                        };
                        prev = current;
                        break;
                }
            }

            if (repeatCount > 1)
            {
                writer.Write((byte)(prev | REPEAT_OVERLAY));
                writer.Write((byte)repeatCount);
            }
            else if (prev != INVALID) writer.Write((byte)prev);

            writer.Flush();
            fs.Flush();
        }


        public static void Decode<T>(string path, T[] outData, SteleMap<T> map, int bufferLength = 16384) where T : struct
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
            byte buffer;

            while ((bytesRead = reader.Read(largeBuffer, 0, largeBuffer.Length)) > 0)
            {
                for (int bufferIndex = 0; bufferIndex < bytesRead; bufferIndex++)
                {
                    buffer = largeBuffer[bufferIndex];

                    for (int i = 0; i < 4; i++)
                    {
                        int halfNib = (buffer >> (i * 2)) & 0b_0000_0011;

                        if (halfNib == 3) // RLE marker (last 2 bits)
                        {
                            //T lastValue = outData[outIndex - 1]; // get last written value.

                            runLength = ((bufferIndex < bytesRead - 1) ? largeBuffer[bufferIndex + 1] : reader.ReadByte()) * 4 + 1;

                            bufferIndex++;
                            Array.Fill(outData, outData[outIndex - 1], outIndex, runLength);

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
