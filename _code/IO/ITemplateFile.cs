using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;

namespace AsitLib.IO
{
    public partial class AsitTemplate
    {
        private TagCollection collection;
        private string content;
        public AsitTemplate(FileInfo file) : this(File.ReadAllText(file.FullName)) { }
        public AsitTemplate(string source)
        {
            content = source;
            collection = new TagCollection(source);
        }
        public TagCollection GetCollection() => collection;
        public string GetContent(TagCollection collection) => TagReplace(content, collection);
    }
    public partial class AsitTemplate
    {
        public static AsitTemplate FromFile(FileInfo file) => new AsitTemplate(File.ReadAllText(file.FullName));
        public static string TagReplace(string source, TagCollection collection)
        {
            Tag[] tags = collection.Tags.ToArray();
            if(tags.Distinct().Count() != tags.Length) throw new ArgumentException(nameof(tags));
            if(tags.Any(t => t.Proxyable)) throw new ArgumentException(nameof(tags));
            if (tags == null || tags.Length == 0) throw new ArgumentException(nameof(tags));

            //get source tags (targetless_)
            IReadOnlyCollection<Tag> sourceTags = new TagCollection(source).Tags;
            foreach (Tag tag in sourceTags)
            {
                //if(tag.Target != null)
                Tag targetFull = tags.Where(t => t.Target == tag.Target).First();
                source = source.Replace("{$\"" + tag.Target + "\"}", targetFull.Value);
            }
            return source;
        }
    }
    public class TagCollection : IReadOnlyCollection<Tag>, IEquatable<TagCollection>
    {
        public IReadOnlyCollection<Tag> Tags { get; }

        public TagCollection(string str) : this(str.Betweens("{$\"", "\"}").ToArray()) { }
        public TagCollection(params string[] strings) : this(strings.Select(s => new Tag(s)).ToArray()) { }
        public TagCollection(params Tag[] tags)
        {
            if (tags == null || tags.Length == 0) throw new ArgumentException(nameof(tags));
            //if (tags.All(t => t.ID != null))
            //{
            //    Tags = tags;
            //}
            //else
            //{
            //    List<Tag> toret = new List<Tag>();
            //    int index = 0;
            //    foreach (var tag in tags)
            //        toret.Add(new Tag(tag.Value, index++));
            //    Tags = toret.ToArray();
            //}
            Tags = tags;
        }
        public IEnumerator<Tag> GetEnumerator() => ((IEnumerable<Tag>)Tags).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Tags.GetEnumerator();
        public int Count => Tags.Count;
        public bool Equals([AllowNull] TagCollection other) => Tags.SequenceEqual(other!.Tags);
        public override bool Equals(object? obj) => obj != null && obj is TagCollection paragraph && Equals(paragraph);
        public static bool operator ==(TagCollection left, TagCollection right) => left.Equals(right);
        public static bool operator !=(TagCollection left, TagCollection right) => !(left == right);
        public override int GetHashCode() => Tags.ToJoinedString().GetHashCode();
        public override string ToString() => Tags.ToJoinedString(", ");

        //public static implicit operator Tag[](TagCollection tags) => tags.Tags.ToArray();
    }
    public readonly struct Tag : IEquatable<Tag>
    {
        public string Value { get; }
        public string Target { get; }
        public Tag(string target) : this(target, target) { }
        public Tag(string value, string target)
        {
            Value = value;
            Target = target;
        }
        public bool Proxyable => Value == Target;
        //public bool Equals([AllowNull] Tag other) => ID == other.ID && Value == other.Value; 
        //public override bool Equals(object? obj) => obj != null && obj is Tag tag && Equals(tag);
        //public static bool operator ==(Tag left, Tag right) => left.Equals(right);
        //public static bool operator !=(Tag left, Tag right) => !(left == right);
        //public override int GetHashCode() => HashCode.Combine(ID, Value);
        //public override string ToString() => "{value: " + Value + ", ID: " + ID + "}";
        //public static explicit operator string(Tag tag) => tag.Value;
        //public static explicit operator Tag(string str) => new Tag(str);


        public bool Equals([AllowNull] Tag other) => Target == other.Target && Value == other.Value;
        public override bool Equals(object? obj) => obj != null && obj is Tag tag && Equals(tag);
        public static bool operator ==(Tag left, Tag right) => left.Equals(right);
        public static bool operator !=(Tag left, Tag right) => !(left == right);
        public override int GetHashCode() => HashCode.Combine(Target, Value);
        public override string ToString() => "{value: " + Value + ", Target: " + Target + "}";
    }
}
