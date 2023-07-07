using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace AsitLib.IO
{
    public readonly partial struct Paragraph : IEquatable<Paragraph>, IRepresentable
    {
        public Header Header { get; }
        public string Content { get; }
        public string SourceContent => Header.ToString() + "\n" + Content;
        public string PrimaryContent { get; }

        public AsitDocument? Source { get; }
        public ReadOnlyCollection<Paragraph> Paragraphs { get;  }
        public Paragraph(string content, AsitDocument? source = null) : this(new Header(content.NormalizeLE().Split('\n').First()), content.NormalizeLE().Split('\n')[1..].ToJoinedString("\n"), source) { }
        public Paragraph(Header header, string content, AsitDocument? source = null)
        {
            content = content.Trim().NormalizeLE();
            Header = header;
            Content = content;
            Source = source;
            PrimaryContent = string.Empty;
            //set unique content>

            string[] lines = (Header.ToString() + "\n" + Content).Split("\n");
            bool reading = false;
            StringBuilder toret = new StringBuilder();
            foreach (string line in lines)
                if (Header.IsValid(line))
                {
                    //Console.WriteLine(line + " for " + header.Title + " and " + reading );
                    if (!reading) reading = true;
                    else break;
                }
                else if (reading) toret.Append(line + "\n");
            PrimaryContent = toret.ToString()[..^1];

            //Set paragraphs>

            //Set reader.
            StreamReader reader = new StreamReader(content.ToStream());

            //Set fast changing values.
            List<string> strparagraphs = new List<string>();
            Header? pheader = null;
            while (true)
            {
                //Set line.
                string line = reader.ReadLine()!;
                if (line == null) break;
                line = line.Trim(); 

                pheader = Header.TryGet(line);
                if (pheader.HasValue && pheader.Value.Depth == Header.Depth + 1) strparagraphs.Add(line + "\n");
                else if(strparagraphs.Count > 0) strparagraphs[^1] += line + "\n";//Add the line to the content of the last paragraph in the list.


            }
            Paragraphs = new ReadOnlyCollection<Paragraph>(strparagraphs.Select(a => new Paragraph(a, source)).ToList()); //Its recursive! :D

        }
        public ParagraphPath GetPath()
        {
            static int[] Rec(ReadOnlyCollection<Paragraph> paragraphs, Header header, int[] indexes)
            {
                for (int i = 0; i < paragraphs.Count; i++)
                    if (paragraphs[i].Header == header) return indexes.Append(i).ToArray(); //Self is found so path is found.
                    else if (paragraphs[i].Paragraphs.Count != 0) return Rec(paragraphs[i].Paragraphs, header, indexes.Append(i).ToArray()); //Self not found so do recursive stuff.
                throw new Exception();
            }
            if (Source == null) throw new InvalidOperationException();
            else return new ParagraphPath(Rec(Source.Paragraphs, Header, Array.Empty<int>()), Source);
        }
        [return: NotNull]
        public string ToRepresentative() => Header.ToString() + '\n' + Content;
        public bool Equals([AllowNull] Paragraph other) => Header.ToString() == other.Header.ToString() && other.Content == Content;
        public override bool Equals(object? obj) => obj != null && obj is Paragraph paragraph && Equals(paragraph);
        public static bool operator ==(Paragraph left, Paragraph right) => left.Equals(right);
        public static bool operator !=(Paragraph left, Paragraph right) => !(left == right);
        public override int GetHashCode() => HashCode.Combine(Header.GetHashCode(), Content.GetHashCode());
        public override string ToString() => Header.ToString();
    }
    public readonly partial struct Paragraph
    {
        public static string GetContentBetweenLines(string input, Func<string, bool> predicate)
        {
            string[] lines = input.Split("\n");
            bool reading = false;
            StringBuilder toret = new StringBuilder();
            foreach(string line in lines)
            {
                if(predicate.Invoke(line))
                {
                    if(!reading) 
                        reading = true;
                    else return toret.ToString()[..^1];
                }
                else if(reading) toret.Append(line + "\n");
            }
            throw new Exception();
        }
    }
}
