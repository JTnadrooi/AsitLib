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
    public sealed partial class AsitDocumentFile : IRepresentable, IEquatable<AsitDocumentFile>, IAsitDocument
    {
        private readonly AsitDocument source;
        private readonly FileInfo fileinfo;
        private readonly Encoding encoding;

        //public AsitDocumentFile(ITemplate template, TagCollection tags) 

        public AsitDocumentFile(string path) : this(path, Encoding.UTF8) { }
        public AsitDocumentFile(FileInfo file) : this(file, Encoding.UTF8) { }
        public AsitDocumentFile(string path, Encoding encoding) : this(new FileInfo(path), encoding) { }
        public AsitDocumentFile(FileInfo file, Encoding encoding)
        {
            fileinfo = file;
            this.encoding = encoding;
            source = new AsitDocument(File.ReadAllText(file.FullName, encoding));
        }
        public AsitDocument BaseDocument => source;

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
        //public static AsitFileData GetAsitFileData() => new AsitFileData(new Version(1,0), Encoding.UTF8, new AsitFileExtension(".aift"), "Asit-Formatted-Text-File");
    }
}
