
namespace AsitLib.Tests
{
    [TestClass]
    public class SteleTests
    {
        [TestMethod]
        [DataRow(0, 8, 8)]
        [DataRow(255, 8, 8)]
        [DataRow(1, 3, 8)]
        [DataRow(1, 8, 3)]
        [DataRow(1, 0, 0)]
        public void ReadMetadata_InvalidData_ThrowsEx(int version, int width, int height)
        {
            MemoryStream stream = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(stream);

            writer.Write((byte)version);
            writer.Write((ushort)width);
            writer.Write((ushort)height);
            stream.Position = 0;

            Invoking(() => SteleData.ReadMetadata(new BinaryReader(stream), out int _, out int _, out int _)).Should().Throw<InvalidDataException>();
        }

        [TestMethod]
        [DataRow(1, 64, 200)]
        [DataRow(1, 8, 8)]
        public void ReadMetadata_ValidData_ReadsValues(int version, int width, int height)
        {
            MemoryStream stream = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(stream);

            writer.Write((byte)version);
            writer.Write((ushort)width);
            writer.Write((ushort)height);
            stream.Position = 0;

            SteleData.ReadMetadata(new BinaryReader(stream), out int actualVersion, out int actualWidth, out int actualHeight);

            actualVersion.Should().Be(version);
            actualWidth.Should().Be(width);
            actualHeight.Should().Be(height);
        }

        [TestMethod]
        [DataRow(64, 128)]
        public void EncodeDecode_RandomValidData_PreservesData(int width, int height)
        {
            Random random = new Random(15);
            SteleMap<byte> map = SteleMap<byte>.CreateFromUnique([(byte)20, (byte)21, (byte)22]);

            byte[] dataIn = new byte[width * height];
            byte[] dataOut = new byte[dataIn.Length];
            MemoryStream stream = new MemoryStream();

            for (int i = 0; i < dataIn.Length; i++)
            {
                dataIn[i] = random.PickFrom(map.InverseMap);
            }

            SteleData<byte>.Encode(stream, dataIn, width, height, map);
            stream.Position = 0;
            SteleData<byte>.Decode(stream, dataOut, map);

            dataOut.Should().Equal(dataIn);
        }

        [TestMethod]
        [DataRow(64, 128)]
        public void EncodeDecode_MonotoneValidData_PreservesData(int width, int height)
        {
            SteleMap<byte> map = SteleMap<byte>.CreateFromUnique([(byte)1]);

            byte[] dataIn = new byte[width * height];
            byte[] dataOut = new byte[dataIn.Length];
            MemoryStream stream = new MemoryStream();

            Array.Fill(dataIn, (byte)1);

            SteleData<byte>.Encode(stream, dataIn, width, height, map);
            stream.Position = 0;
            SteleData<byte>.Decode(stream, dataOut, map);

            dataOut.Should().Equal(dataIn);
        }

        [TestMethod]
        public void Decode_InvalidDataSize_ThrowsEx()
        {
            int width = 8;
            int height = 8;

            byte[] invalidOutput = new byte[(width * height) - 1];
            SteleMap<byte> map = SteleMap<byte>.CreateFromUnique([(byte)0, (byte)11, (byte)12]);

            MemoryStream stream = new MemoryStream();
            SteleData<byte>.Encode(stream, new byte[width * height], width, height, map);
            stream.Position = 0;

            Invoking(() => SteleData<byte>.Decode(stream, invalidOutput, map)).Should().Throw<ArgumentException>();
        }
    }

    [TestClass]
    public class SteleMapTests
    {
        [TestMethod]
        [DataRow((byte[])[])]
        [DataRow((byte[])[1, 2, 2])]
        public void CreateFromUnique_InvalidData_ThrowsEx(byte[] values)
        {
            Invoking(() => SteleMap<byte>.CreateFromUnique(values)).Should().Throw<ArgumentException>();
        }

        [TestMethod]
        [DataRow((byte[])[1])]
        [DataRow((byte[])[1, 2])]
        [DataRow((byte[])[1, 2, 3])]
        public void CreateFromUnique_ValidData_CreatesMap(byte[] values)
        {
            SteleMap<byte> map = SteleMap<byte>.CreateFromUnique(values);

            map.Should().NotBeNull();
        }
    }
}
