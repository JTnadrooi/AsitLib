using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;

namespace AsitLib.IO
{
    public readonly partial struct ParagraphPath : IEquatable<ParagraphPath>
    {
        public int[] Indexes { get; }
        public AsitDocument? Source { get; }
        public ParagraphPath(params int[] indexes) : this(indexes, null) { }
        public ParagraphPath(int[] indexes, AsitDocument? source = null)
        {
            Source = source;
            Indexes = indexes;
        }
        public string[] GetTitles()
        {
            if(Source == null) throw new ArgumentNullException("source");
            List<string> toret = new List<string>();
            for (int i = 0; i < Indexes.Length; i++)
                toret.Add(Source.GetParagraph(new ParagraphPath(Indexes[..(i + 1)], Source)).Header.Title);
            return toret.ToArray();
        }
        public ParagraphPath Raise(int amount) // needs testing
            => new ParagraphPath(Indexes[(..^amount)], Source);
        public ParagraphPath Lower(params int[] indexes) // needs testing
            => new ParagraphPath(Indexes.Concat(indexes).ToArray(), Source);

        public bool Equals([AllowNull] ParagraphPath other) => Source == other.Source && Indexes.SequenceEqual(other.Indexes);
        public override bool Equals(object? obj) => obj != null && obj is ParagraphPath && Equals((ParagraphPath)obj);
        public static bool operator ==(ParagraphPath left, ParagraphPath right) => left.Equals(right);
        public static bool operator !=(ParagraphPath left, ParagraphPath right) => !(left == right);
        public override int GetHashCode() => HashCode.Combine(Indexes.ToJoinedString(), Source);
        public override string ToString() => Indexes.ToJoinedString("\\");
    }
    public readonly partial struct ParagraphPath
    {
        public static ParagraphPath Empty => new ParagraphPath(Array.Empty<int>());
        public static ParagraphPath Top => Empty;
        public static ParagraphPath EmptyFrom(AsitDocument? source) => new ParagraphPath(Array.Empty<int>(), source);
    }
}
