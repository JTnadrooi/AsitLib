using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib.IO
{
    public struct ParagraphTemplate : IRepresentable
    {
        public List<ParagraphTemplate> Paragraphs { get; set; }
        public string PrimaryContent { get; set; }
        public string Title { get; set; }
        public string[]? Attributes { get; set; }
        public ParagraphTemplate(Paragraph source)
        {
            //List<TempParagraph> tempParagraphs = new List<TempParagraph>();
            //void Rec(Paragraph currentParagraph)
            //{
            //    if (currentParagraph.Paragraphs.Count == 0) return;
            //    else foreach (Paragraph sub in currentParagraph.Paragraphs)
            //            Rec(sub);
            //}
            //Paragraphs = ;
            ParagraphTemplate Rec(Paragraph p)
            {
                //var class2 = new TempParagraph(new List<TempParagraph>(),p.Content, p.Header.Title, p.Header.Attributes);
                //Console.WriteLine(p.Header.Title + "<<");
                //Console.WriteLine(p.UniqueContent);
                var class2 = new ParagraphTemplate(p.Paragraphs.Select(p => Rec(p)).ToList(), p.PrimaryContent, p.Header.Title, p.Header.Attributes);
                //if (source.Paragraphs.Count == 0) return class2;
                //for (int i = 0; i < p.Paragraphs.Count; i++)
                //{
                //    class2.Paragraphs.Add(Rec(p.Paragraphs[i]));
                //}
                return class2;
            }
            ParagraphTemplate temp = Rec(source);

            Paragraphs = temp.Paragraphs;
            PrimaryContent = temp.PrimaryContent;
            Title = temp.Title;
            Attributes = temp.Attributes;
        }
        public ParagraphTemplate() : this(new List<ParagraphTemplate>(), string.Empty, string.Empty, Array.Empty<string>()) { }
        public ParagraphTemplate(List<ParagraphTemplate> tempParagraphs, string content, string title, string[]? attributes = null)
        {
            Paragraphs = tempParagraphs;
            PrimaryContent = content;
            Title = title;
            Attributes = attributes;
        }
        public override string ToString() => Title;
        public string ToString(int depth) => new Header(Title, depth, Attributes).ToString() + "\n" + PrimaryContent;
        [return: NotNull]
        public string ToRepresentative() => GetContent();
        public string GetContent()
        {
            StringBuilder stringBuilder = new StringBuilder();
            void Rec(ParagraphTemplate currentParagraph, int depth)
            {
                stringBuilder.Append(currentParagraph.ToString(depth) + "\n");
                if (currentParagraph.Paragraphs.Count == 0) return;
                else foreach (ParagraphTemplate sub in currentParagraph.Paragraphs)
                        Rec(sub, depth + 1);
            }
            Rec(this, 0);
            return stringBuilder.ToString()[..^1];
        }
    }
}
