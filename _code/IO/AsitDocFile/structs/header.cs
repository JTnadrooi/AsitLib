using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
#nullable enable

namespace AsitLib.IO
{
    public readonly partial struct Header : ICloneable, IEquatable<Header>, IRepresentable
    {
        public string Title { get; }
        public int Depth { get; }
        public string[]? Attributes { get; }
        public string Source { get; }
        public Header(string title, int depth, string[]? attributes)
            : this(
                  attributes == null ?
                  new StringBuilder("[" + depth.ToString() + "|" + title + "]").Append(';').ToString() :
                  new StringBuilder("[" + depth.ToString() + "|" + title + "]").AppendJoin(" : ", attributes).Append(';').ToString())
        { }
        public Header(string source) // [2|title] : attribute1 : attribute2
        {
            Source = source;
            source = source[..^1];
            if(source.Between("[", "]").Contains('|')) // syntax sugar
            {
                Depth = int.Parse(source.Between("[", "]").Split('|')[0]);
                Title = source.Between("[", "]").Split('|')[1];
            }
            else
            {
                Depth = 0;
                Title = source.Between("[", "]");
            }
            if (Depth == 0) Source = "[" + source[3..];
            if (Depth < 0) throw new ArgumentException("Depth is negative.");
            Attributes = source.Contains(" : ") ? source.Split(" : ")[1..] : null;
        }
        public override string ToString() => Source;
         //=> "[" + new IntManipulator().Maniputate(Depth, "punc") + Title + "]" + (Attribute == null || Attribute == String.Empty ? "" : "(" + Attribute + ")");

        public object Clone() => new Header();
        public bool Equals([AllowNull] Header other)
            => other.Source == Source;
        public override bool Equals(object? obj)
            => obj != null && obj is Header header && Equals(header);
        public static bool operator ==(Header left, Header right) => left.Equals(right);
        public static bool operator !=(Header left, Header right) => !(left == right);
        public override int GetHashCode() => Source.GetHashCode();

        [return: NotNull]
        public string ToRepresentative() => ToString();
    }
    public readonly partial struct Header
    {
        public static Header? TryGet(string source)
            => IsValid(source) ? new Header(source) : null;
        public static bool IsValid(string line)
            => Regex.IsMatch(line.Trim().Trim('\n', '\r'), @"(\[\w+\|[^\]]+\] : [^;]+;)|(\[[^\[\]]+\]\s*:\s*[^;]+;\s*)");
        public static Header Empty => new Header(string.Empty, 0, Array.Empty<string>());
    }
}
