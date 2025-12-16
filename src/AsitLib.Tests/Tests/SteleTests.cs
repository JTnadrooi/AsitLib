using AsitLib.Chance;
using AsitLib.CommandLine;
using AsitLib.Stele;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib.Tests
{
    [TestClass]
    public class SteleTests
    {
        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {

        }

        [ClassCleanup]
        public static void ClassCleanup()
        {

        }

        [TestMethod]
        [DataRow(0, 8, 8)]
        [DataRow(255, 8, 8)]
        [DataRow(1, 3, 8)]
        [DataRow(1, 8, 3)]
        [DataRow(1, 0, 0)]
        public void ReadMetadata_InvalidData(int version, int width, int height)
        {
            MemoryStream mockStream = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(mockStream);

            writer.Write((byte)version);
            writer.Write((ushort)width);
            writer.Write((ushort)height);
            mockStream.Position = 0;

            Assert.Throws<InvalidDataException>(() => SteleData.ReadMetadata(new BinaryReader(mockStream), out int actualVersion, out int actualWidth, out int actualHeight));
        }

        [TestMethod]
        [DataRow(1, 64, 200)]
        [DataRow(1, 8, 8)]
        public void ReadMetadata_ValidData(int version, int width, int height)
        {
            MemoryStream mockStream = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(mockStream);

            writer.Write((byte)version);
            writer.Write((ushort)width);
            writer.Write((ushort)height);
            mockStream.Position = 0;

            SteleData.ReadMetadata(new BinaryReader(mockStream), out int actualVersion, out int actualWidth, out int actualHeight);
            Assert.AreEqual(version, actualVersion);
            Assert.AreEqual(width, actualWidth);
            Assert.AreEqual(height, actualHeight);
        }

        [TestMethod]
        [DataRow(64, 128)]
        public void EncodeDecode_RandomValidData(int width, int height)
        {
            Random r = new Random(15);
            SteleMap<byte> map = SteleMap<byte>.CreateFromUnique([(byte)20, (byte)21, (byte)22]);
            byte[] dataIn = new byte[width * height];
            using MemoryStream memoryStream = new MemoryStream();
            byte[] dataOut = new byte[dataIn.Length];

            for (int i = 0; i < dataIn.Length; i++)
            {
                dataIn[i] = r.PickFrom(map.InverseMap);
            }
            //Console.WriteLine(dataIn.ToJoinedString());

            SteleData<byte>.Encode(memoryStream, dataIn, width, height, map);
            memoryStream.Position = 0;
            SteleData<byte>.Decode(memoryStream, dataOut, map);

            CollectionAssert.AreEqual(dataIn, dataOut);
        }

        [TestMethod]
        [DataRow(64, 128)]
        public void EncodeDecode_MonotoneValidData(int width, int height)
        {
            Random r = new Random(15);
            SteleMap<byte> map = SteleMap<byte>.CreateFromUnique([(byte)1]);
            byte[] dataIn = new byte[width * height];
            using MemoryStream memoryStream = new MemoryStream();
            Array.Fill(dataIn, (byte)1);
            byte[] dataOut = new byte[dataIn.Length];

            SteleData<byte>.Encode(memoryStream, dataIn, width, height, map);
            memoryStream.Position = 0;
            SteleData<byte>.Decode(memoryStream, dataOut, map);

            CollectionAssert.AreEqual(dataIn, dataOut);
        }

        [TestMethod]
        public void Decode_InvalidDataSize()
        {
            int width = 8;
            int height = 8;
            byte[] data = new byte[(width * height) - 1];
            SteleMap<byte> map = SteleMap<byte>.CreateFromUnique([(byte)0, (byte)11, (byte)12]);

            MemoryStream memoryStream = new MemoryStream();
            SteleData<byte>.Encode(memoryStream, new byte[width * height], width, height, map);
            memoryStream.Position = 0;

            Assert.Throws<ArgumentException>(() => SteleData<byte>.Decode(memoryStream, data, map));
        }
    }

    [TestClass]
    public class SteleMapTests
    {
        [TestMethod]
        [DataRow((byte[])[])]
        [DataRow((byte[])[1, 2, 2])]
        public void CreateFromUnique_InvalidData(byte[] values)
        {
            Assert.Throws<ArgumentException>(() => SteleMap<byte>.CreateFromUnique(values));
        }

        [TestMethod]
        [DataRow((byte[])[1])]
        [DataRow((byte[])[1, 2])]
        [DataRow((byte[])[1, 2, 3])]
        public void CreateFromUnique_ValidData(byte[] values)
        {
            SteleMap<byte> map = SteleMap<byte>.CreateFromUnique(values);
        }
    }
}

