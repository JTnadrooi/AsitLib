using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib
{
    public readonly partial struct AsitFileExtension 
    {
        public string Full => Segments.ToJoinedString('.');
        public string Primary => Segments.Count >= 1 ? Segments.ElementAt(^1) : throw new InvalidOperationException();
        public string Secondary => Segments.Count >= 2 ? Segments.ElementAt(^2) : throw new InvalidOperationException();

        public IReadOnlyCollection<string> Segments { get; }
        public AsitFileExtension(FileInfo file) : this(file.Name.Split('.')[1..]) { }
        public AsitFileExtension(string full) : this(full.Split('.')) { }
        public AsitFileExtension(string[] Segments)
        {
            //if(Segments.Length == 0) throw new ArgumentException(nameof(Segments));
            this.Segments = Segments.ToList().AsReadOnly();
        }

        public bool IsSegmented => Segments.Count > 1;

        public override string ToString() => Full;
    }
    public readonly partial struct AsitFileExtension
    {
        public static AsitFileExtension None => new AsitFileExtension(Array.Empty<string>());
        public static AsitFileExtension Empty => None;
    }
}
