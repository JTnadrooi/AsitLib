using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib.IO
{
    /// <summary>
    /// Indroduces a accesable way to write a <see cref="AsitDocument"/>.
    /// </summary>
    public sealed partial class AsitDocumentWriter
    {
        private string content;
        public ParagraphTemplate upper;
        public List<ParagraphTemplate> Paragraphs => upper.Paragraphs;
        /// <summary>
        /// The <see cref="Title"/> of this <see cref="AsitDocumentWriter"/>.
        /// </summary>
        public string Title
        {
            get => upper.Title;
            set => upper.Title = value;
        }
        /// <summary>
        /// The <see cref="PrimaryContent"/> of this <see cref="AsitDocumentWriter"/>, this <see cref="string"/> isn't shared by any child 
        /// <see cref="Paragraph"/> objects.
        /// </summary>
        public string PrimaryContent
        {
            get => upper.PrimaryContent; 
            set => upper.PrimaryContent = value;
        }
        /// <summary>
        /// Create a new <see cref="AsitDocumentWriter"/> with set Title and set Primary content. 
        /// </summary>
        /// <param name="Title"></param>
        /// <param name="primaryContent"></param>
        public AsitDocumentWriter(string Title, string primaryContent = "") 
        { 
            content = string.Empty;
            upper  = new ParagraphTemplate();
            upper.Title = Title;
            upper.PrimaryContent = primaryContent;
        }
        public ParagraphPath Add(ParagraphTemplate paragraph, ParagraphPath path)
        {
            //get paragraph to add this to.
            
            ParagraphTemplate currentParagraph = upper; //Pls, future nadrooi don't make TempParagraph a reference type.
            foreach (int index in path.Indexes) currentParagraph = currentParagraph.Paragraphs[index];
            currentParagraph.Paragraphs.Add(paragraph);
            return path.Lower(currentParagraph.Paragraphs.Count - 1);
        }
        public void Reset() => upper = new ParagraphTemplate();
        public string Export()
        {
            return upper.GetContent();
        }
        public void ExportTo(out string str) => str = Export();
        public void ExportTo(string path, Encoding encoding) => ExportTo(new FileInfo(path), encoding);
        public void ExportTo(FileInfo file, Encoding encoding) => File.WriteAllText(file.FullName, Export(), encoding);
        public AsitDocument ToDocument() => new AsitDocument(Export());
        public AsitDocumentFile ToFile(string path, Encoding encoding)
        {
            File.WriteAllText(path, Export());
            return new AsitDocumentFile(new FileInfo(path), encoding);
        }
    }
}
