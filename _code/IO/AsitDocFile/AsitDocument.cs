using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;

namespace AsitLib.IO
{
    public sealed partial class AsitDocument : IEquatable<AsitDocument>, ICloneable, IAsitDocument
    {
        public Header Header { get; }
        public ReadOnlyCollection<Paragraph> Paragraphs { get; }
        public string Content { get; }
        public string SourceContent => Header.ToString() + "\n" + Content;

        private readonly Paragraph upper;

        public AsitDocument(string content)
        {
            Content = content.NormalizeLE().Trim();
            upper = new Paragraph(Content, this);
            Header = upper.Header;
            Paragraphs = upper.Paragraphs;
        }
        public Paragraph GetParagraph(ParagraphPath path)
        {
            if (path.Source != this) throw new InvalidOperationException();
            Paragraph currentParagraph = upper; //Pls, future nadrooi don't make Paragraph a reference type.
            foreach (int index in path.Indexes) currentParagraph = currentParagraph.Paragraphs[index];
            return currentParagraph;
        }
        //public bool TryGetParagraph(string path, out Paragraph? paragraph)
        //    => AsitLibStatic.EasyTryOut<Action, Paragraph?>(() => GetParagraph(path), out paragraph);
        [return: NotNull]
        public string ToRepresentative() => SourceContent;
        public object Clone() => new AsitDocument(SourceContent);
        public bool Equals(AsitDocument? other) => other != null && other.SourceContent == SourceContent;
        public override bool Equals(object? obj) => Equals(obj as AsitDocument);
        public override int GetHashCode() => HashCode.Combine(Content.GetHashCode(), Header.GetHashCode());

        public void Dispose() { }
    }
}
