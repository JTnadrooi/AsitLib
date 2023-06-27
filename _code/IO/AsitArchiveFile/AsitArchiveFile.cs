using AsitLib;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;

namespace AsitLib.IO
{
    /// <summary>
    /// A object pointing to a file amaglamation.
    /// </summary>
    public class AsitArchiveFile : IDisposable
    {
        /// <summary>
        /// Version of this <see cref="AsitArchiveFile"/>.
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// User defined attribule set tot this <see cref="AsitArchiveFile"/>.
        /// </summary>
        public string Attribute { get; set; }
        /// <summary>
        /// Path of the file this <see cref="AsitArchiveFile"/> points to.
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// Size of the file.
        /// </summary>
        public long Size
        {
            get
            {
                FileStream fs = File.OpenRead(Path);
                long _size = fs.Length;
                fs.Dispose();
                return _size;
            }
        }
        /// <summary>
        /// Point to a <see cref="AsitArchiveFile"/> located at the given <paramref name="fileInfo"/>.
        /// </summary>
        /// <param name="fileInfo">Path to the file.</param>
        public AsitArchiveFile(FileInfo fileInfo) : this(fileInfo.FullName) { }
        /// <summary>
        /// Point to a <see cref="AsitArchiveFile"/> located at the given <paramref name="path"/>.
        /// </summary>
        /// <param name="path">Location of the file.</param>
        /// <exception cref="ArgumentException"></exception>
        public AsitArchiveFile(string path)
        {
            if (!File.Exists(path)) throw new ArgumentException("Pointed File Doesn't Exist.");
            Path = path;
        }
        /// <summary>
        /// Extract this <see cref="AsitArchiveFile"/> to a folder.
        /// </summary>
        /// <param name="extractTo">Folder to extract this <see cref="AsitArchiveFile"/> to.</param>
        /// <param name="config"><see cref="AsitFileConfig"/> used to extract.</param>
        public void Extract(string extractTo, AsitFileExtractConfig config) => AsitArchiveUtils.Extract(this.Path, config, extractTo);
        /// <summary>
        /// Validate the interity(?) of this <see cref="AsitArchiveFile"/>.
        /// </summary>
        /// <returns>A value indicating if the <see cref="AsitArchiveFile"/> is valid.</returns>
        public bool Validate() => AsitArchiveUtils.Validate(Path);
        public void Dispose() => GC.SuppressFinalize(this);
    }
    public static class AsitFileConfig
    {
        public static string Seperator = "?SPLIT?";
        public static string Starter = "<<<<";
        public static string Finisher = ">>>>";
    }
    /// <summary>
    /// Configuration for the extraction of <see cref="AsitArchiveFile"/> objects.
    /// </summary>
    public struct AsitFileExtractConfig
    {
        /// <summary>
        /// A <see cref="string"/> that seperates the filename and the filespace.
        /// </summary>
        public string Seperator { get; set; }
        /// <summary>
        /// A <see cref="string"/> that indicates a (new)filespace has started.
        /// </summary>
        public string Starter { get; set; }
        /// <summary>
        /// A <see cref="string"/> that indicates a filespace has ended.
        /// </summary>
        public string Finisher { get; set; }
        /// <summary>
        /// Method used to extract a <see cref="AsitArchiveFile"/>.
        /// </summary>
        public AsitArchiveUtils.ExtractMethod ExtractMethod { get; set; }
        /// <summary>
        /// Create a new <see cref="AsitFileExtractConfig"/> with all values set.
        /// </summary>
        /// <param name="seperator"><see cref="Seperator"/>.</param>
        /// <param name="starter"><see cref="Starter"/>.</param>
        /// <param name="finisher"><see cref="Finisher"/>.</param>
        /// <param name="extractMethod"><see cref="ExtractMethod"/>.</param>
        public AsitFileExtractConfig(string seperator, string starter, string finisher, AsitArchiveUtils.ExtractMethod extractMethod)
        {
            Seperator = seperator;
            Starter = starter;
            Finisher = finisher;
            ExtractMethod = extractMethod;
        }
    }
    /// <summary>
    /// Configuration for the creation of <see cref="AsitArchiveFile"/> objects.
    /// </summary>
    public struct AsitFilePackageConfig
    {
        /// <summary>
        /// A <see cref="string"/> that seperates the filename and the filespace.
        /// </summary>
        public string Seperator { get; set; }
        /// <summary>
        /// A <see cref="string"/> that indicates a (new)filespace has started.
        /// </summary>
        public string Starter { get; set; }
        /// <summary>
        /// A <see cref="string"/> that indicates a filespace has ended.
        /// </summary>
        public string Finisher { get; set; }
        /// <summary>
        /// The version of the <see cref="AsitArchiveFile"/>.
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// A custom <see cref="AsitArchiveFile.Attribute"/> added to the <see cref="AsitArchiveFile"/>.
        /// </summary>
        public string Attribute { get; set; }
    }
    /// <summary>
    /// A collection of utils used for the <see cref="AsitArchiveFile"/> object.
    /// </summary>
    public static partial class AsitArchiveUtils
    {
        /// <summary>
        /// Method used to extract a <see cref="AsitArchiveFile"/>.
        /// </summary>
        public enum ExtractMethod
        {
            /// <summary>
            /// Extract a <see cref="AsitArchiveFile"/> via a method thats <see langword="RAM"/> dependent.<br/>
            /// <i>This method is often slower than the alternate <see cref="CpuBased"/> option.</i>
            /// </summary>
            RamBased,
            /// <summary>
            /// Extract a <see cref="AsitArchiveFile"/> via a method thats <see langword="CPU"/> dependent. <br/>
            /// <i>This method is often faster than the alternate <see cref="RamBased"/> option.</i>
            /// </summary>
            CpuBased,
        }
        //public static void Operate(string[] commandParts)
        //{
        //    string addittion = string.Empty;
        //    if (commandParts.Length > 1) addittion = commandParts[1];
        //    Used.LogAsit("Access.Operate Called..");
        //    switch (commandParts[0])
        //    {
        //        case "Package":Package(addittion); break;
        //        case "RAMExtract":Extract(addittion, ExtractMethod.RamBased, addittion[..^(Path.GetExtension(addittion).Count())] + " - asitRAM\\"); break;
        //        case "CPUExtract":Extract(addittion, ExtractMethod.CpuBased, addittion[..^(Path.GetExtension(addittion).Count())] + " - asitCPU\\"); break;
        //    }
        //}
        /// <summary>
        /// Validate a <see cref="AsitArchiveFile"/>.
        /// </summary>
        /// <param name="path">Path to the <see cref="AsitArchiveFile"/>.</param>
        /// <returns>A value indicating if the <see cref="AsitArchiveFile"/> is valid.</returns>
        public static bool Validate(string path)
        {
            string attributes = string.Empty;
            using FileStream asitF_Stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            for (int i = 0; i < 26; i++) attributes += (char)(byte)asitF_Stream.ReadByte();
            if (attributes[8] == '|' && attributes[17] == '|') return true;
            else return false;
        }
        //static public AsitArchiveFile Package(string orginDirectory, string destination = "", byte asitVerID = 255, string customDecoder = "", string attribute = "abc")
        static public AsitArchiveFile Package(string orginDirectory, string destination = "", byte asitVerID = 255, string customDecoder = "", string attribute = "abc")
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            if (!orginDirectory.EndsWith("\\")) orginDirectory += "\\";
            if (destination == "") destination = orginDirectory[..^1] + ".asitc";
            if (customDecoder.ToByteArray().Length > 8 || attribute.ToByteArray().Length > 8) throw new Exception("Invalid new AsitFile properties. (STRING_TO_LONG)");
            string S_AsitTypeID = asitVerID.ToString();
            while (S_AsitTypeID.ToByteArray().Length < 8) S_AsitTypeID = ":" + S_AsitTypeID;
            string S_Attribute = attribute;
            while (S_Attribute.ToByteArray().Length < 8) S_Attribute = ":" + S_Attribute;
            string S_customDecoder = customDecoder;
            while (S_customDecoder.ToByteArray().Length < 8) S_customDecoder = ":" + S_customDecoder;
            if (!File.Exists(destination)) File.Create(destination).Dispose();
            using FileStream AsitFile = new FileStream(destination, FileMode.Open, FileAccess.Write);
            AsitFile.Write((S_AsitTypeID + "|" + S_Attribute + "|" + S_customDecoder).ToByteArray());
            ConvertDirectory(orginDirectory);
            void ConvertDirectory(string _directory)
            {
                foreach (string path in Directory.GetFiles(_directory))
                {
                    AsitFile.Write((AsitFileConfig.Starter + path.Replace(orginDirectory, "") + AsitFileConfig.Seperator).ToByteArray());
                    using FileStream dirFile = new FileStream(path, FileMode.Open, FileAccess.Read);
                    int l = (int)dirFile.Length;
                    byte[] buffer = new byte[1024];
                    int bytesRead = 0;
                    while ((bytesRead = dirFile.Read(buffer, 0, 1024)) > 0) AsitFile.Write(buffer, 0, bytesRead);
                    AsitFile.Write(AsitFileConfig.Finisher.ToByteArray());
                }
                foreach (string directory in Directory.GetDirectories(_directory)) ConvertDirectory(directory);
            }
            timer.Stop();
            //Console.WriteLine(timer.ElapsedMilliseconds);
            return new AsitArchiveFile(destination);
        }
        //static public void Extract(string in_asitPath, ExtractMethod extractMethod, string extract_to = "")
        static public void Extract(string in_asitPath, AsitFileExtractConfig config, string extract_to = "")
        {
            if(config.ExtractMethod == ExtractMethod.RamBased)
            {
                if (!File.Exists(in_asitPath)) throw new IOException("AsitFile doesn't exist.");
                if (!Validate(in_asitPath)) throw new IOException("Invalid AsitFile.");
                if (extract_to == "") extract_to = in_asitPath[..^Path.GetExtension(in_asitPath).Length] + "\\";
                Stopwatch timer = new Stopwatch();
                timer.Start();
                if (!extract_to.EndsWith("\\")) extract_to += "\\";
                if (Directory.Exists(extract_to))
                {
                    foreach (FileInfo file in new DirectoryInfo(extract_to).EnumerateFiles()) file.Delete();
                    foreach (DirectoryInfo dir in new DirectoryInfo(extract_to).EnumerateDirectories()) dir.Delete(true);
                    Directory.Delete(extract_to);
                }
                Directory.CreateDirectory(extract_to);
                if (!File.Exists(in_asitPath)) throw new IOException();
                byte[] asitCont = File.ReadAllBytes(in_asitPath);
                string asitContent = string.Join("", asitCont[26..].Select(b => (char)b));
                string[] MEG_attributes = string.Join("", asitCont[..26].Select(b => (char)b)).Split("|");
                string AsitVer = MEG_attributes[0], attribute = MEG_attributes[1], decoder = MEG_attributes[2];
                if (asitContent == String.Empty) throw new Exception("Empty file.");
                foreach (string for_file in asitContent.Split(AsitFileConfig.Finisher + AsitFileConfig.Starter))
                {
                    string file = for_file;
                    if (file.StartsWith(config.Starter)) file = file[(AsitFileConfig.Starter.Length)..];
                    if (file.EndsWith(config.Finisher)) file = file[..^(AsitFileConfig.Finisher.Length)];
                    file = file.Split(config.Seperator)[0] + AsitFileConfig.Seperator + file.Split(config.Seperator)[1];
                    Directory.CreateDirectory(extract_to + file.Split(AsitFileConfig.Seperator)[0].Replace(Path.GetFileName(extract_to + file.Split(AsitFileConfig.Seperator)[0]), "\\"));
                    File.Create(extract_to + file.Split(config.Seperator)[0]).Dispose();
                    File.WriteAllBytes(extract_to + file.Split(config.Seperator)[0], file.Split(AsitFileConfig.Seperator)[1].ToByteArray());
                }
                timer.Stop();
            }
            else if(config.ExtractMethod == ExtractMethod.CpuBased)
            {
                if (!File.Exists(in_asitPath)) throw new IOException("AsitFile doesn't exist.");
                if (!Validate(in_asitPath)) throw new IOException("Invalid AsitFile.");
                if (extract_to == "") extract_to = in_asitPath[..^Path.GetExtension(in_asitPath).Length] + "\\";
                Stopwatch timer = new Stopwatch();
                timer.Start();
                char[] memory = new char[] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' };
                if (Directory.Exists(extract_to))
                {
                    foreach (FileInfo file in new DirectoryInfo(extract_to).EnumerateFiles()) file.Delete();
                    foreach (DirectoryInfo dir in new DirectoryInfo(extract_to).EnumerateDirectories()) dir.Delete(true);
                    Directory.Delete(extract_to);
                }
                Directory.CreateDirectory(extract_to);
                if (!File.Exists(in_asitPath)) throw new IOException();
                using FileStream asitF_Stream = new FileStream(in_asitPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                FileStream MEGstream = null;
                string rd_file = string.Empty;
                bool Bfile_rd = false, proccesContent = false;
                string attributes = string.Empty;
                for (int i = 0; i < 26; i++)
                {
                    byte b = (byte)asitF_Stream.ReadByte();
                    attributes += (char)b;
                }
                string[] MEG_attributes = attributes.Split('|');
                string AsitVer = MEG_attributes[0], attribute = MEG_attributes[1], decoder = MEG_attributes[2];
                int l = (int)asitF_Stream.Length;
                string valueChecker = new string(new char[] { config.Starter[0], config.Finisher[0], config.Seperator[0] });
                for (int i = 0; i < l; i++)
                {
                    byte bcurrent = (byte)asitF_Stream.ReadByte();
                    char current = (char)bcurrent;
                    if (proccesContent && rd_file != String.Empty) MEGstream.WriteByte(bcurrent);
                    memory[0] = memory[1];
                    memory[1] = memory[2];
                    memory[2] = memory[3];
                    memory[3] = memory[4];
                    memory[4] = memory[5];
                    memory[5] = memory[6];
                    memory[6] = memory[7];
                    memory[7] = current;
                    if (Bfile_rd) rd_file += current;
                    if (valueChecker.Contains(current))
                        switch (new string(memory)) //new string is 50%!
                        {
                            case string s when s.EndsWith(config.Starter):
                                Bfile_rd = true;
                                break;
                            case string s when s.EndsWith(config.Finisher):
                                Bfile_rd = false;
                                proccesContent = false;
                                rd_file = string.Empty;
                                break;
                            case string s when s.EndsWith(config.Seperator):
                                Bfile_rd = false;
                                rd_file = rd_file[..^(config.Seperator.Length)];
                                Directory.CreateDirectory(Path.GetDirectoryName(extract_to + rd_file));
                                File.Create(extract_to + rd_file).Dispose();
                                if (MEGstream != null) MEGstream.Dispose();
                                MEGstream = new FileStream(extract_to + rd_file, FileMode.Append);
                                proccesContent = true;
                                break;
                        }
                }
                if (MEGstream != null) MEGstream.Dispose();
                foreach (string file in Directory.EnumerateFiles(extract_to, "*.*", SearchOption.AllDirectories))
                {
                    FileStream fs = new FileStream(file, FileMode.Open, FileAccess.ReadWrite);
                    fs.SetLength(fs.Length - config.Finisher.Length);
                    fs.Close();
                    fs.Dispose();
                }
                timer.Stop();
            }
            
        }
        //static public void Extract(string in_asitPath, ExtractMethod extractMethod, string extract_to = "")
        //{
        //    if (extractMethod == ExtractMethod.RamBased)
        //    {
        //        if (!File.Exists(in_asitPath)) throw new IOException("AsitFile doesn't exist.");
        //        if (!Validate(in_asitPath)) throw new IOException("Invalid AsitFile.");
        //        if (extract_to == "") extract_to = in_asitPath[..^Path.GetExtension(in_asitPath).Length] + "\\";
        //        Stopwatch timer = new Stopwatch();
        //        timer.Start();
        //        if (!extract_to.EndsWith("\\")) extract_to += "\\";
        //        if (Directory.Exists(extract_to))
        //        {
        //            foreach (FileInfo file in new DirectoryInfo(extract_to).EnumerateFiles()) file.Delete();
        //            foreach (DirectoryInfo dir in new DirectoryInfo(extract_to).EnumerateDirectories()) dir.Delete(true);
        //            Directory.Delete(extract_to);
        //        }
        //        Directory.CreateDirectory(extract_to);
        //        if (!File.Exists(in_asitPath)) throw new IOException();
        //        byte[] asitCont = File.ReadAllBytes(in_asitPath);
        //        string asitContent = string.Join("", asitCont[26..].Select(b => (char)b));
        //        string[] MEG_attributes = string.Join("", asitCont[..26].Select(b => (char)b)).Split("|");
        //        string AsitVer = MEG_attributes[0], attribute = MEG_attributes[1], decoder = MEG_attributes[2];
        //        if (asitContent == String.Empty) throw new Exception("Empty file.");
        //        foreach (string for_file in asitContent.Split(AsitFileConfig.Finisher + AsitFileConfig.Starter))
        //        {
        //            string file = for_file;
        //            if (file.StartsWith(AsitFileConfig.Starter)) file = file[(AsitFileConfig.Starter.Length)..];
        //            if (file.EndsWith(AsitFileConfig.Finisher)) file = file[..^(AsitFileConfig.Finisher.Length)];
        //            file = file.Split(AsitFileConfig.Seperator)[0] + AsitFileConfig.Seperator + file.Split(AsitFileConfig.Seperator)[1];
        //            Directory.CreateDirectory(extract_to + file.Split(AsitFileConfig.Seperator)[0].Replace(Path.GetFileName(extract_to + file.Split(AsitFileConfig.Seperator)[0]), "\\"));
        //            File.Create(extract_to + file.Split(AsitFileConfig.Seperator)[0]).Dispose();
        //            File.WriteAllBytes(extract_to + file.Split(AsitFileConfig.Seperator)[0], file.Split(AsitFileConfig.Seperator)[1].ToByteArray());
        //        }
        //        timer.Stop();
        //    }
        //    else if (extractMethod == ExtractMethod.CpuBased)
        //    {
        //        if (!File.Exists(in_asitPath)) throw new IOException("AsitFile doesn't exist.");
        //        if (!Validate(in_asitPath)) throw new IOException("Invalid AsitFile.");
        //        if (extract_to == "") extract_to = in_asitPath[..^Path.GetExtension(in_asitPath).Length] + "\\";
        //        Stopwatch timer = new Stopwatch();
        //        timer.Start();
        //        char[] memory = new char[] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' };
        //        if (Directory.Exists(extract_to))
        //        {
        //            foreach (FileInfo file in new DirectoryInfo(extract_to).EnumerateFiles()) file.Delete();
        //            foreach (DirectoryInfo dir in new DirectoryInfo(extract_to).EnumerateDirectories()) dir.Delete(true);
        //            Directory.Delete(extract_to);
        //        }
        //        Directory.CreateDirectory(extract_to);
        //        if (!File.Exists(in_asitPath)) throw new IOException();
        //        using FileStream asitF_Stream = new FileStream(in_asitPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        //        FileStream MEGstream = null;
        //        string rd_file = string.Empty;
        //        bool Bfile_rd = false, proccesContent = false;
        //        string attributes = string.Empty;
        //        for (int i = 0; i < 26; i++)
        //        {
        //            byte b = (byte)asitF_Stream.ReadByte();
        //            attributes += (char)b;
        //        }
        //        string[] MEG_attributes = attributes.Split('|');
        //        string AsitVer = MEG_attributes[0], attribute = MEG_attributes[1], decoder = MEG_attributes[2];
        //        int l = (int)asitF_Stream.Length;
        //        string valueChecker = new string(new char[] { AsitFileConfig.Starter[0], AsitFileConfig.Finisher[0], AsitFileConfig.Seperator[0] });
        //        for (int i = 0; i < l; i++)
        //        {
        //            byte bcurrent = (byte)asitF_Stream.ReadByte();
        //            char current = (char)bcurrent;
        //            if (proccesContent && rd_file != String.Empty) MEGstream.WriteByte(bcurrent);
        //            memory[0] = memory[1];
        //            memory[1] = memory[2];
        //            memory[2] = memory[3];
        //            memory[3] = memory[4];
        //            memory[4] = memory[5];
        //            memory[5] = memory[6];
        //            memory[6] = memory[7];
        //            memory[7] = current;
        //            if (Bfile_rd) rd_file += current;
        //            if (valueChecker.Contains(current))
        //                switch (new string(memory)) //new string is 50%!
        //                {
        //                    case string s when s.EndsWith(AsitFileConfig.Starter):
        //                        Bfile_rd = true;
        //                        break;
        //                    case string s when s.EndsWith(AsitFileConfig.Finisher):
        //                        Bfile_rd = false;
        //                        proccesContent = false;
        //                        rd_file = string.Empty;
        //                        break;
        //                    case string s when s.EndsWith(AsitFileConfig.Seperator):
        //                        Bfile_rd = false;
        //                        rd_file = rd_file[..^(AsitFileConfig.Seperator.Length)];
        //                        Directory.CreateDirectory(Path.GetDirectoryName(extract_to + rd_file));
        //                        File.Create(extract_to + rd_file).Dispose();
        //                        if (MEGstream != null) MEGstream.Dispose();
        //                        MEGstream = new FileStream(extract_to + rd_file, FileMode.Append);
        //                        proccesContent = true;
        //                        break;
        //                }
        //        }
        //        if (MEGstream != null) MEGstream.Dispose();
        //        foreach (string file in Directory.EnumerateFiles(extract_to, "*.*", SearchOption.AllDirectories))
        //        {
        //            FileStream fs = new FileStream(file, FileMode.Open, FileAccess.ReadWrite);
        //            fs.SetLength(fs.Length - AsitFileConfig.Finisher.Length);
        //            fs.Close();
        //            fs.Dispose();
        //        }
        //        timer.Stop();
        //    }

        //}
    }
}
