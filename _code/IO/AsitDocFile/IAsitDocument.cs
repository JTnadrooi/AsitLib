using System;
using System.Collections.ObjectModel;

namespace AsitLib.IO
{
    public interface IAsitDocument : IRepresentable, IDisposable
    {
        string Content { get; }
        Header Header { get; }
        ReadOnlyCollection<Paragraph> Paragraphs { get; }
        string SourceContent { get; }
        Paragraph GetParagraph(ParagraphPath path);
    }
}