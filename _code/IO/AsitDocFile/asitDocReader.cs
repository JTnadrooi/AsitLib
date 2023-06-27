using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;

namespace AsitLib.IO
{
    /// <summary>
    /// A Stream to read string organized.
    /// </summary>
    public class DocReader : DocReaderBase<DocParagraph, DocKey, DocStatement>
    {
        /// <summary>
        /// If created from file this returns a Path to that file, else this returns String.Empty.
        /// </summary>
        public string Path { get; internal set; }
        /// <summary>
        /// Create a DocStream from a file.
        /// </summary>
        /// <param name="file"></param>
        /// <exception cref="ArgumentException"></exception>
        public DocReader(FileInfo file) : this(new StreamReader(new FileStream(file.FullName, FileMode.Open, FileAccess.Read), Encoding.UTF8))
         => Path = file.FullName;
        public DocReader(string str) : base(str) { }
        public DocReader(StreamReader reader) : base(reader) { }
        public DocReader(byte[] str, Encoding e) : base(str, e) { }
        public override void Dispose()
        {
            Path = null;
        }
    }
    public interface IDocReader<out ParagraphType, out KeyType, out StatementType> : IDisposable where ParagraphType : DocParagraphBase<ParagraphType, KeyType, StatementType>, new() where KeyType : DocKeyBase, new() where StatementType : DocStatementBase, new()
    {
        /// <summary>
        /// All the <see cref="DocKeyBase"/> implementing objects this AsitDocParagraph holds.
        /// </summary>
        public KeyType[] Keys { get; }
        /// <summary>
        /// All the <see cref="DocStatementBase"/> implementing objects this AsitDocParagraph holds.
        /// </summary>
        public StatementType[] Statements { get; }
        /// <summary>
        /// All the <see cref="DocParagraphBase{ParagraphType, KeyType, StatementType}"/> implementing objects this AsitDocParagraph holds.
        /// </summary>
        public ParagraphType[] Paragraphs { get; }
        /// <summary>
        /// The <see cref="Attribute"/> of this <see cref="IDocReader{ParagraphType, KeyType, StatementType}"/>.
        /// </summary>
        public string Attribute { get; }
        /// <summary>
        /// The <see cref="Title"/> of this <see cref="IDocReader{ParagraphType, KeyType, StatementType}"/>.
        /// </summary>
        public string Title { get; }
        /// <summary>
        /// Get the size of this Paragraph in
        /// </summary>
        /// <returns></returns>
        public long GetSize();
        public ParagraphType GetParagraphFromPath(string path);
    }
    public abstract class DocReaderBase<ParagraphType, KeyType, StatementType> : IDocReader<ParagraphType, KeyType, StatementType> where ParagraphType : DocParagraphBase<ParagraphType, KeyType, StatementType>, new() where KeyType : DocKeyBase, new() where StatementType : DocStatementBase, new()
    {
        protected long streamPosAtBeginRead;
        protected StreamReader usingReader;
        public KeyType[] Keys { get; protected internal set; }
        public StatementType[] Statements { get; protected internal set; }
        public string Attribute { get; internal set; }
        public string Title { get; protected internal set; }
        public ParagraphType[] Paragraphs { get; protected internal set; }
        public DocReaderBase(string str) : this(new StreamReader(str.ToStream())) { }
        public DocReaderBase(byte[] str, Encoding e) : this(new StreamReader(e.GetString(str).ToStream(), e)) { }
        public DocReaderBase(StreamReader reader) => Initialize(reader);
        protected virtual void Initialize(StreamReader reader)
        {
            //Console.WriteLine(reader.BaseStream.Position);
            //Todo; setup properties. >>
            streamPosAtBeginRead = reader.BaseStream.Position;
            usingReader = reader;
            ResetReader();
            Title = AsitDocUtils.GetTitle(reader.ReadLine(), "");
            ResetReader();
            Attribute = AsitDocUtils.GetTitleDesc(reader.ReadLine(), String.Empty);
            ResetReader();

            Paragraphs = GetParagraphs();
            Keys = GetKeys();

        }
        protected virtual ParagraphType[] GetParagraphs()
        {
            ParagraphType[] toret;
            ResetReader();
            toret = AsitDocUtils.GetParagraphs<ParagraphType>(usingReader).ToArray();
            ResetReader();
            return toret;
        }
        protected virtual KeyType[] GetKeys()
        {
            KeyType[] toret;
            ResetReader();
            toret = AsitDocUtils.GetKeysAndStatements<KeyType, StatementType>(usingReader.ReadToEnd()).Keys.ToArray();
            ResetReader();
            return toret;
        }
        public virtual long GetSize()
        {
            long size = 0;
            foreach (ParagraphType p in Paragraphs) SizeOfPar(p);
            void SizeOfPar(DocParagraphBase<ParagraphType, KeyType, StatementType> par)
            {
                foreach (KeyType key in par.Keys) size += key.GetSize();
                size += Encoding.UTF8.GetBytes(par.ToRepresentativeString()).LongLength;
                foreach (ParagraphType sub in par.Subs) SizeOfPar(sub);
            }
            return size;
        }
        /// <summary>
        /// Searches this Documents paragraph hierachy for a Sub Paragraph given a path.
        /// </summary>
        /// <param name="path">Path to the Paragraph.</param>
        /// <returns>A Paragraph with the designied path.</returns>
        public virtual ParagraphType GetParagraphFromPath(string path) => AsitDocUtils.GetParagraphFromPath<ParagraphType>(path, Paragraphs);
        public abstract void Dispose();
        protected void ResetReader()
        {
            usingReader.BaseStream.Position = streamPosAtBeginRead;
            usingReader.DiscardBufferedData();
        }
    }
}
