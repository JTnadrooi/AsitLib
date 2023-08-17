using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace AsitLib.IO
{
    public static class AsitFile
    {
        public static AsitFileData? TryGetAsitFileData(string path)
        {
            try
            {
                return AsitFileData.FromFile(path);
            }
            catch { return null; }
        }
        public static bool IsAsitFile(string path) => TryGetAsitFileData(path) != null;
        public static void WriteAllText(string path, string content)
        {
            if (!File.Exists(path)) File.Create(path).Dispose();
            if (IsAsitFile(path)) File.WriteAllText(path, AsitFileData.FromFile(new FileInfo(path)).ToString() + content);
            else File.WriteAllText(path, content);
        }
        public static FileStream CreateFromTemplate(string path, AsitFileData data, AsitTemplate template, TagCollection templateInput)
        {
            Create(path, data).Dispose();
            WriteAllText(path, template.GetContent(templateInput));
            return new FileStream(path, FileMode.Open, FileAccess.ReadWrite);
        }

        public static FileStream Create(string path, AsitFileData data)
        {
            File.WriteAllText(path, data.ToString(), Encoding.ASCII);
            return new FileStream(path, FileMode.Open, FileAccess.ReadWrite);
        }

        public static string ReadAllText(string path) => ReadAllText(path, Encoding.UTF8);
        public static string ReadAllText(string path, Encoding e)
        {
            return IsAsitFile(path) ? e.GetString(File.ReadAllBytes(path)[(AsitFileData.FromFile(path).Length)..]) : File.ReadAllText(path, e);
        }
    }
    public readonly partial struct AsitFileData
    {
        private int SkippedBytes { get;  }
        public Version Version { get; }//1
        public Encoding Encoding { get; }//2
        public AsitFileExtension CoreExtension { get; }//3
        public string FileType { get; }//4
        public int Length => SkippedBytes + ToString().Length; // for bom

        public AsitFileData(string version, string e, string coreExtension, string fileType) : this(version, e, coreExtension, fileType, 0) { }
        public AsitFileData(string version, string e, string coreExtension, string fileType, int skippedBytes)
            : this(new Version(version), Encoding.GetEncoding(e)
                  /*(Encoding)(Activator.CreateInstance(Type.GetType("System.Text." + e + "Encoding+" + e + "EncodingSealed") ?? throw new Exception()) ?? throw new Exception())*/,
                  new AsitFileExtension(coreExtension.Split('.')),
            fileType, skippedBytes)
        {
            if (e.Length != Encoding.GetEncoding(e).BodyName.Length) throw new  ArgumentException(
                    "\"" + e + "\"" + "(input encoding name) is invalid because \"" + e + "\" does it not match the lenght of the BodyName. " +
                    "Use \"" + Encoding.GetEncoding(e).BodyName + "\" instead."
                    );
        }
        public AsitFileData(Version version, Encoding e, AsitFileExtension coreExtension, string fileType) : this(version, e, coreExtension, fileType, 0) { }
        public AsitFileData(Version version, Encoding e, AsitFileExtension coreExtension, string fileType, int skippedBytes)
        {
            Version = version;
            Encoding = e;
            CoreExtension = coreExtension;
            FileType = fileType;
            SkippedBytes = skippedBytes;
        }
        public override string ToString()
            => "#" + new string[] { Version.ToString(), Encoding.HeaderName, CoreExtension.ToString(), FileType }.ToJoinedString("#") + "#";
    }
    public readonly partial struct AsitFileData
    {
        public static AsitFileData Empty => new AsitFileData(new Version(0, 0), Encoding.UTF8, AsitFileExtension.Empty, string.Empty);
        public static AsitFileData FromFile(string path) => FromFile(new FileInfo(path));
        public static AsitFileData FromFile(FileInfo file)
        {
            using FileStream stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read);
            //if (stream.ReadByte() != (byte)'#') throw new Exception();

            int startIndex = -1;
            for (int i = 0; i < 10; i++)
            {
                int b = stream.ReadByte();
                if ((byte)b == (byte)'#')
                {
                    startIndex = i;
                    break;
                }
                if(b == -1 && startIndex == -1) throw new Exception();
            }
            if (startIndex == -1) throw new Exception();
            stream.Seek(startIndex + 1, SeekOrigin.Begin);
            Console.WriteLine(startIndex);
            //for (int i = 0; (i = stream.ReadByte()) != 0 && (byte)i != '\uFEFF' && i != '\u200B';)
            //{
            //    int i = stream.ReadByte();


            //}

            int segment = 0;


            string[] segments = new string[] { string.Empty, string.Empty, string.Empty, string.Empty, };

            for (int i = 0; (segment != 4 && (i = stream.ReadByte()) != -1);)
            {

                char c = (char)i;
                //if (segment > 3) break;
                //Console.Write(c);
                segments[segment] += c;
                if (c == '#') segment++;

            }
            //Console.Write('\n');
            segments = segments.Select(s => s[..^1]).ToArray();
            Console.WriteLine(segments.ToJoinedString(", "));
            return new AsitFileData(segments[0], segments[1], segments[2], segments[3], startIndex);
        }
    }
}
