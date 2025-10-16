using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static AsitLib.Stele.SteleData;

namespace AsitLib.Stele
{
    public static class SteleData
    {
        public const string FILE_EXTENSION = "stele";
        public const int VERSION = 0;
        public const int DEFAULT_BUFFER_SIZE = 2 ^ 13; // 8192
    }

    public class SteleData<TPixel> : IDisposable where TPixel : struct, IEquatable<TPixel>
    {
        public FileInfo? FileInfo => _lazyFileInfo?.Value;
        public int Width { get; }
        public int Height { get; }
        public float Ratio { get; }
        public int FileSize { get; }
        public int Count { get; }
        public int Depth => 2;

        //public int GetRawSize(int bbp)
        //{

        //}

        private Stream _source;
        private BinaryReader _reader;
        private Lazy<FileInfo>? _lazyFileInfo;
        private bool disposedValue;

        public SteleData(string path, FileMode mode, int bufferSize = DEFAULT_BUFFER_SIZE) : this(path, new FileStreamOptions()
        {
            Mode = mode,
            BufferSize = bufferSize,
            Access = FileAccess.Write,
        })
        { }
        public SteleData(string path, FileStreamOptions options) : this(new FileStream(path, options)) { }
        public SteleData(FileStream source) : this((Stream)source)
        {
            _lazyFileInfo = new Lazy<FileInfo>(() => new FileInfo(source.Name));
        }
        public SteleData(Stream source)
        {
            _source = source;
            _reader = new BinaryReader(_source);
            _lazyFileInfo = null;
        }

        public void GetData(TPixel[] outData, SteleMap<TPixel> map, int bufferSize = DEFAULT_BUFFER_SIZE)
        {
            GetData(_source, outData, map, bufferSize);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _source.Dispose();
                    _reader.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public static void Encode(string path, TPixel[] data, int width, int height, SteleMap<TPixel> map, int bufferSize = DEFAULT_BUFFER_SIZE)
            => Encode(path, FileMode.Open, data, width, height, map, bufferSize);
        public static void Encode(string path, FileMode mode, TPixel[] data, int width, int height, SteleMap<TPixel> map, int bufferSize = DEFAULT_BUFFER_SIZE)
        {
            using FileStream fs = new FileStream(path, new FileStreamOptions()
            {
                BufferSize = bufferSize,
                Mode = mode,
                Access = FileAccess.Write,
            });
            Encode(fs, data, width, height, map, bufferSize);
        }

        public static void Encode(Stream source, TPixel[] data, int width, int height, SteleMap<TPixel> map, int bufferSize = DEFAULT_BUFFER_SIZE)
        {
            using BinaryWriter writer = new BinaryWriter(source);
            Encode(writer, data, width, height, map, bufferSize);
        }

        public static void Encode(BinaryWriter writer, TPixel[] data, int width, int height, SteleMap<TPixel> map, int bufferSize = DEFAULT_BUFFER_SIZE)
        {
            if ((width % 4) != 0 || width < 4) throw new ArgumentException(nameof(width));
            if ((height % 4) != 0 || height < 4) throw new ArgumentException(nameof(height));

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
            writer.BaseStream.Flush();
        }

        public static void GetData(string path, TPixel[] outData, SteleMap<TPixel> map, int bufferSize = DEFAULT_BUFFER_SIZE)
        {
            using FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            GetData(fs, outData, map, bufferSize);
        }

        public static void GetData(Stream source, TPixel[] outData, SteleMap<TPixel> map, int bufferSize = DEFAULT_BUFFER_SIZE)
        {
            using BinaryReader reader = new BinaryReader(source);
            GetData(reader, outData, map, bufferSize);
        }

        public static void GetData(BinaryReader reader, TPixel[] outData, SteleMap<TPixel> map, int bufferSize = DEFAULT_BUFFER_SIZE)
        {
            byte version = reader.ReadByte();
            ushort width = reader.ReadUInt16();
            ushort height = reader.ReadUInt16();

            if (outData.Length != width * height) throw new ArgumentException(nameof(outData));

            byte[] buffer = new byte[Math.Min(outData.Length / 4, bufferSize)];
            int outIndex = 0;
            int bytesRead;
            int runLength;
            byte current;

            while ((bytesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
            {
                for (int bufferIndex = 0; bufferIndex < bytesRead; bufferIndex++)
                {
                    current = buffer[bufferIndex];

                    for (int i = 0; i < 4; i++)
                    {
                        int halfNib = (current >> (i * 2)) & 0b_0000_0011;

                        if (halfNib == 3) // RLE marker (last 2 bits)
                        {
                            runLength = ((bufferIndex < bytesRead - 1) ? buffer[bufferIndex + 1] : reader.ReadByte()) * 4 + 1;

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
