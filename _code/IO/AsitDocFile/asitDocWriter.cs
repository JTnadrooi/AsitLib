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
    /// Write to a AsitDocument.
    /// Reads file into memory. 
    /// </summary>
    public abstract class DocWriterBase : IDisposable
    {
        /// <summary>
        /// All the paragraphs in this Document.
        /// </summary>
        public DocParagraph[] Paragraphs { get; internal set; }
        /// <summary>
        /// Title of the AsitDocument.
        /// </summary>
        public string Title { internal set; get; }
        /// <summary>
        /// If this writer originates from a File, this will return a path.
        /// </summary>
        public string Path { internal set; get; }
        public DocReader BaseReader { internal set; get; }
        private FileSystemWatcher _watcher;
        private StreamWriter _writer;
        private FileInfo _info;
        private bool disposedValue;
        ///// <summary>
        ///// Get Content of Writer.
        ///// </summary>
        //public string Content
        //{
        //    get
        //    {
        //        if (Path == String.Empty) return _content;
        //        else return File.ReadAllText(Path);
        //    }
        //    internal set
        //    {
        //        if (Path == String.Empty) _content = value;
        //        else throw new InvalidOperationException("Invalid mode for operation.");
        //    }
        //}
        public virtual string GetContent() => File.ReadAllText(Path);
        public DocWriterBase(string path) : this(new FileInfo(path)) { }
        public DocWriterBase(FileInfo path)
        {
            _info = path;
            Path = path.FullName;
            Initialize();
        }
        protected virtual void Initialize()
        {
            Paragraphs = AsitDocUtils.GetParagraphs<DocParagraph>(File.ReadAllText(Path));
            Title = System.IO.Path.GetFileNameWithoutExtension(Path);
            try
            {
                if (AsitDocUtils.IsTitle(File.ReadLines(Path).First()))
                {
                    Title = File.ReadLines(Path).First().Between("[", "]");
                }
            }
            catch
            {
                Title = System.IO.Path.GetFileNameWithoutExtension(Path);
            }
            //foreach (var par in Subs)
            //{
            //    Console.WriteLine(par.Title);
            //}
            if(!File.ReadAllText(Path).EndsWith("\n") && _info.Length > 0) File.WriteAllText(Path, File.ReadAllText(Path) + "\n");
            _watcher = new FileSystemWatcher
            {
                Path = System.IO.Path.GetDirectoryName(Path),
                Filter = System.IO.Path.GetFileName(Path),
                EnableRaisingEvents = true
            };
            _watcher.Changed += Update;
            if (_writer != null) _writer.AutoFlush = true; //because there is no flush() method anymore

        }

        private void Update(object sender, FileSystemEventArgs e) //for title method
        {
            _info = new FileInfo(this.Path);
        }
        /// <summary>
        /// Set the Title of this document without changing the file name.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="attribute"></param>
        /// <param name="general_text"></param>
        /// <exception cref="ArgumentException"></exception>
        public virtual void SetTitle(string title, string attribute, string general_text)
        {
            string line = "[" + title + "]";
            if (attribute != null) line += "(" + attribute + ")";
            if (!AsitDocUtils.IsTitle(line)) throw new ArgumentException("Invalid title.");
            if (_info.Length == 0)
            {
                if (general_text != null) File.WriteAllText(Path, line + "\n" + general_text + "\n");
                else File.WriteAllText(Path, line + "\n");
            }
            else if (!AsitDocUtils.IsTitle(File.ReadLines(Path).First()))
            {
                if (general_text != null) File.WriteAllText(Path, line + "\n" + general_text + "\n" + File.ReadAllText(Path) + "\n");
                else File.WriteAllText(Path, line + "\n" + File.ReadAllText(Path));
                if (!File.ReadAllText(Path).EndsWith("\n")) File.WriteAllText(Path, File.ReadAllText(Path) + "\n");
            }
            else if (AsitDocUtils.IsTitle(File.ReadLines(Path).First()))
            {
                int lc = 0;
                foreach (string lline in File.ReadAllLines(Path))
                    if (AsitDocUtils.IsHeader(lline)) break;
                    else lc++;
                Used.WriteAllLinesLE(Path, File.ReadAllLines(Path)[lc..], "\n");
                if (general_text != null) File.WriteAllText(Path, line + "\n" + general_text + "\n" + File.ReadAllText(Path) + "\n"); //xclude newline
                else File.WriteAllText(Path, line + "\n" + File.ReadAllText(Path) + "\n");
            }
            Title = title;
        }
        /// <summary>
        /// Add a pararaph to the path with values ripped from a existing paragraph.
        /// </summary>
        /// <param name="paragraph">DocParagraph to get values from.</param>
        /// <param name="paragraphPath"></param>
        /// <returns>The path of the newly created paragraph.</returns>
        public virtual string Annex(DocParagraph paragraph, string paragraphPath) => Annex(paragraph.Path, paragraph.Title, paragraph.ToRepresentativeString(), paragraph.Attribute);
        /// <summary>
        /// Add a pararaph to the path.
        /// </summary>
        /// <param name="title">Title of the paragraph to add.</param>
        /// <param name="content">Content of the paragraph to add.</param>
        /// <param name="attribute"></param>
        /// <param name="paragraphPath">Path for the paragraph to be nestled into.</param>
        /// <returns>The path of the newly created paragraph.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public virtual string Annex(string paragraphPath, string title, string content, string attribute)
        {
            if (title.StartsWith(".") || title.StartsWith(":")) throw new ArgumentException("Invalid title, can't contain '.' and ':'.");
            if (attribute.Contains("(") || attribute.Contains(")")) throw new ArgumentException("Invalid atribute, can't contain '(' and ')'.");

            if (paragraphPath != "0")
                paragraphPath += "\\1";
            int depth = 0;
            string to_create = paragraphPath;
            depth += paragraphPath.Split("\\").Count();
            string l1 = "[" + AsitDocUtils.IntToPunc(depth) + title + "]";
            if(attribute != string.Empty) l1 += "(" + attribute + ")";
            string to_override = to_create.Split("\\")[..^1].ToJoinedString("\\");
            if (to_override == String.Empty || to_create == "0\\1")
            {
                to_override = "0";
                to_create = "1";
            }
            //Console.WriteLine("Par to override; " + to_override);
            //Console.WriteLine("Par to create; " + to_create);
            //Console.WriteLine("First Line; " + l1);
            if(to_override == "0")
            {
                if (depth < 1) throw new ArgumentException("Depth cannot be less than 1.");
                else File.WriteAllText(Path, File.ReadAllText(Path) + l1 + "\n" + content + "\n");
                Paragraphs = AsitDocUtils.GetParagraphs<DocParagraph>(File.ReadAllText(Path));
                return (Paragraphs.Where(p => p.Level == 1).Count() + 1).ToString();
            }
            Paragraphs = AsitDocUtils.GetParagraphs<DocParagraph>(File.ReadAllText(Path));
            DocParagraph ths = AsitDocUtils.GetParagraphFromPath(to_override, Paragraphs);
            if (Paragraphs.Any(p => p.Title == title && p.Level == depth && p.NestedInParagraph == ths.Title)) throw new InvalidOperationException("Paragraph already exists.");
            File.WriteAllText(Path, File.ReadAllText(Path).Replace(ths.ToRepresentativeString(), ths.ToRepresentativeString() + "\n" + l1 + "\n" + content));
            return ths.Path + "\\"+ (ths.Subs.Count() + 1).ToString();
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _watcher.Dispose();
                    _writer.Dispose();
                }
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~DocWriterBase()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
            
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
