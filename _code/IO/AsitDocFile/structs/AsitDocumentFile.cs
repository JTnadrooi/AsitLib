using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib.IO
{
    public sealed partial class AsitDocumentFile : IRepresentable, IEquatable<AsitDocumentFile>, IAsitFile, IAsitDocument
    {
        private readonly AsitDocument source;
        private readonly FileInfo fileinfo;
        private readonly Encoding encoding;

        public AsitDocumentFile(FileInfo file, Encoding encoding)
        {
            fileinfo = file;
            this.encoding = encoding;
            source = new AsitDocument(File.ReadAllText(file.FullName, encoding));
        }
        public AsitDocument BaseDocument => source;

        public Encoding Encoding => encoding;
        public string FileType => "Asit-Formatted-Text-File";
        public int Version => 2;
        public string CoreExtension => ".aift"; //AsitFormatedText
        public object[]? Other => null;
        public FileInfo FileInfo => fileinfo;
        public string Content => source.Content;
        public Header Header => source.Header;
        public ReadOnlyCollection<Paragraph> Paragraphs => source.Paragraphs;
        public string SourceContent => source.SourceContent;

        public Paragraph GetParagraph(ParagraphPath path) => source.GetParagraph(path);
        public void Dispose() => source.Dispose();
        public bool Equals(AsitDocumentFile? other) => other != null && other.GetHashCode() == GetHashCode();
        [return: NotNull]
        public string ToRepresentative() => source.ToRepresentative();
        public override bool Equals(object? obj) => Equals(obj as AsitDocumentFile);
        public override int GetHashCode() => HashCode.Combine(source, fileinfo);
    }
    public sealed partial class AsitDocumentFile
    {
        public static AsitDocumentFile GetFromPath(string path) => GetFromFileInfo(new FileInfo(path));
        public static AsitDocumentFile GetFromPath(string path, Encoding encoding) => GetFromFileInfo(new FileInfo(path), encoding);

        public static AsitDocumentFile GetFromFileInfo(FileInfo source) => GetFromFileInfo(source, Encoding.UTF8);
        public static AsitDocumentFile GetFromFileInfo(FileInfo source, Encoding encoding) => new AsitDocumentFile(source, encoding);
    }
}
