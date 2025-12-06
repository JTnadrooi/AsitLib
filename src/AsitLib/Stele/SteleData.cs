using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

using static AsitLib.Stele.SteleData;

namespace AsitLib.Stele
{
    /// <summary>
    /// Contains static Stele related functions that do not take the TPixel type parameter. 
    /// For functions that do, see the <see cref="SteleData{TPixel}"/> and <see cref="SteleMap{TPixel}"/> static functions.
    /// </summary>
    public static class SteleData
    {
        public const string FILE_EXTENSION = "stele";
        public const int VERSION = 1;
        public const int DEFAULT_BUFFER_SIZE = 8192; // 2 ^ 13

        /// <summary>
        /// Calculated as follows: <code>sizeof(byte) + sizeof(ushort) + sizeof(ushort)</code>
        /// </summary>
        public const int METADATA_SIZE = sizeof(byte) + sizeof(ushort) + sizeof(ushort);

        /// <summary>
        /// Reads the STELE header/metadata.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="throwVersionError">If <see langword="true"/>, this throws an error if the version is invalid or cannot be decoded.</param>
        /// <param name="throwDimensionsError">If <see langword="true"/>, this throws an error if the dimensions are invalid.</param>
        /// <param name="version">The STELE version. Cannot be 0 and must be less than <see cref="byte.MaxValue"/>.</param>
        /// <param name="width">The image width. Cannot be less than 4 and must be divisible by 4.</param>
        /// <param name="height">The image height. Cannot be less than 4 and must be divisible by 4.</param>
        public static void ReadMetadata(BinaryReader reader, out int version, out int width, out int height)
        {
            version = reader.ReadByte();
            width = reader.ReadUInt16();
            height = reader.ReadUInt16();

            switch (version)
            {
                case 0: throw new InvalidDataException("Version 0 is not supported.");
                case > byte.MaxValue: throw new InvalidDataException($"Version {version} exceeds the maximum allowed version ({byte.MaxValue}).");
                case VERSION: break; // only 1 supported version.
                default: throw new InvalidDataException($"Version {version} is invalid.");
            }

            if (width < 4) throw new InvalidDataException($"Width ({width}) cannot be less than 4.");
            if (width % 4 != 0) throw new InvalidDataException($"Width ({width}) must be divisible by 4.");

            if (height < 4) throw new InvalidDataException($"Height ({height}) cannot be less than 4.");
            if (height % 4 != 0) throw new InvalidDataException($"Height ({height}) must be divisible by 4.");
        }
    }

    /// <summary>
    /// TODO: STELE INFO <br/><br/>
    /// Also holds a collection of usefull static helper functions.<br/>
    /// <strong>For reading from multiple Stele data streams, these functions may be faster as they do not allocate a <see cref="SteleData{TPixel}"/> object.</strong>
    /// </summary>
    /// <typeparam name="TPixel">The IEquatable-implementing pixel struct type.</typeparam>
    public class SteleData<TPixel> : IDisposable where TPixel : struct, IEquatable<TPixel>
    {
        /// <summary>
        /// The width of the image.
        /// </summary>
        public int Width => _width;
        /// <summary>
        /// The height of the image.
        /// </summary>
        public int Height => _height;
        /// <summary>
        /// The amount of pixels this SteleData blob stores.
        /// </summary>
        public int PixelCount => Width * Height;
        /// <summary>
        /// The amount of bits per pixel as stored in stele. In the current version this is always <see langword="2"/>.
        /// </summary>
        public int Depth => 2;

        //public int GetRawSize(int bbp) // uncompressed size in bytes
        //    => PixelCount * bbp + METADATA_SIZE;

        //public float GetRatio(int bbp, int size)
        //    => GetRawSize(bbp) / (float)size;

        private Stream _sourceStream;
        private BinaryReader _reader;
        private bool _disposedValue;
        private readonly int _height;
        private readonly int _width;

        public SteleData(string path, int bufferSize = DEFAULT_BUFFER_SIZE) : this(new FileStream(path, new FileStreamOptions()
        {
            Mode = FileMode.Open,
            BufferSize = bufferSize,
            Access = FileAccess.Write,
        }))
        { }

        public SteleData(Stream source)
        {
            _sourceStream = source;
            _reader = new BinaryReader(_sourceStream, Encoding.ASCII, true);

            ReadMetadata(_reader, out _, out _width, out _height);
        }

        public void Decode(TPixel[] outData, SteleMap<TPixel> map, int bufferSize = DEFAULT_BUFFER_SIZE)
        {
            InternalDecode(_reader, outData, map, bufferSize, VERSION, Width, Height);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _sourceStream.Dispose();
                    _reader.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public static void Encode(string path, TPixel[] data, int width, int height, SteleMap<TPixel> map, int bufferSize = DEFAULT_BUFFER_SIZE)
        {
            using FileStream fs = new FileStream(path, new FileStreamOptions()
            {
                BufferSize = bufferSize,
                Mode = FileMode.Create,
                Access = FileAccess.Write,
            });
            Encode(fs, data, width, height, map, bufferSize);
        }

        public static void Encode(Stream source, TPixel[] data, int width, int height, SteleMap<TPixel> map, int bufferSize = DEFAULT_BUFFER_SIZE)
        {
            using BinaryWriter writer = new BinaryWriter(source, Encoding.ASCII, true); // encoding irrelevant.

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

        public static void Decode(string path, TPixel[] outData, SteleMap<TPixel> map, int bufferSize = DEFAULT_BUFFER_SIZE)
        {
            using FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            Decode(fs, outData, map, bufferSize);
        }

        public static void Decode(Stream source, TPixel[] outData, SteleMap<TPixel> map, int bufferSize = DEFAULT_BUFFER_SIZE)
        {
            using BinaryReader reader = new BinaryReader(source, Encoding.ASCII, true);
            InternalDecode(reader, outData, map, bufferSize);
        }

        private static void InternalDecode(BinaryReader reader, TPixel[] outData, SteleMap<TPixel> map, int bufferSize = DEFAULT_BUFFER_SIZE, int version = 0, int width = -1, int height = -1)
        {
            if (version == 0)
            {
                ReadMetadata(reader, out version, out width, out height);
            }

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
