using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;

namespace AsitLib.IO
{
    ///// <summary>
    ///// <para><strong>Title:</strong>  Search by Paragraph Title.</para>
    ///// <para><strong>ID:</strong>  Search by Paragraph ID.</para>
    ///// <para><strong>Hybrid:</strong> Search by ID and if that fails resort to Title.</para>
    ///// </summary>
    //public enum DocSearchType
    //{
    //    /// <summary>
    //    /// Search by Paragraph Title.
    //    /// </summary>
    //    Title,
    //    /// <summary>
    //    /// Search by Paragraph ID.
    //    /// </summary>
    //    ID,
    //    /// <summary>
    //    /// Search by ID and if that fails resort to Title.
    //    /// </summary>
    //    Hybrid,
    //}
    /// <summary>
    /// Static class to provide some basic DocUtils.
    /// </summary>
    public static class AsitDocUtils
    {
        //Not done.
        public static DocReader CreateDocument(string directoryPath, string outpath = null, string[] whiteList = null)
        {
            if (outpath == null)
                if (directoryPath.EndsWith("\\")) outpath = directoryPath[..^1] + ".asitd";
                else outpath = directoryPath + ".asitd";
            whiteList ??= new string[] { ".txt" };
            FileStream fs = File.Create(outpath);
            StreamWriter sw = new StreamWriter(fs);
            sw.AutoFlush = true;
            sw.WriteLine("[" + new DirectoryInfo(directoryPath).Name + "]");
            ExFolder(directoryPath, 1);
            void ExFolder(string folderpath, int depth)
            {
                foreach(string file in Directory.GetFiles(folderpath).Where(s => whiteList.Any(e => s.EndsWith(e))))
                {
                    sw.WriteLine("[" + IntToPunc(depth) + Path.GetFileNameWithoutExtension(file) + "]");
                    sw.WriteLine(File.ReadAllText(file));
                }
                foreach(string directory in Directory.GetDirectories(folderpath))
                {
                    sw.WriteLine("[" + IntToPunc(depth) + Directory.GetParent(directory + "\\a.a").Name + "]");
                    ExFolder(directory, depth + 1);
                }
            }
            sw.Dispose();
            fs.Dispose();
            return new DocReader(new FileInfo(outpath));
        }
        public static ParagraphType[] GetParagraphs<ParagraphType>(string str) where ParagraphType : IDocParagraph<IDocParagraph, DocKeyBase, DocStatementBase>, new() => GetParagraphs<ParagraphType>(new StreamReader(str.ToStream()));
        public static ParagraphType[] GetParagraphs<ParagraphType>(StreamReader reader) where ParagraphType : IDocParagraph<IDocParagraph, DocKeyBase, DocStatementBase>, new()
        {

            string line;
            string contentBuffer = string.Empty;
            List<ParagraphType> paragraphs = new List<ParagraphType>();
            void this_Cast()
            {
                Console.WriteLine(contentBuffer);
                Console.WriteLine("-----------------");
                contentBuffer = String.Empty;
                ParagraphType newe = new ParagraphType();
                MethodInfo privMethod = newe.GetType().GetMethod("PrivateMethodName", BindingFlags.NonPublic | BindingFlags.Instance);
                //privMethod.Invoke(newe, new object[] { "path", "nestedInParagraph", contentBuffer, "nestedInTop", "filePath" });
                paragraphs.Add(newe);
            }
            while ((line = reader.ReadLine()) != null)
            {
                if (AsitDocUtils.IsHeader(line))
                {
                    this_Cast();
                }
                contentBuffer += "\n" + line;
            }
            this_Cast();
            return paragraphs.ToArray();
        }
        public static string CleanParagraphPath(string path)
        {
            for (; path.EndsWith("\\0"); path = path[..^2]) { }
            return path;
        }
        public static string GetTitle(string titleLine, string @else)
        {
            if (!IsTitle(titleLine)) return @else;
            else return titleLine.Between("[", "]");
        }
        public static string GetTitleDesc(string titleLine, string @else)
        {
            if (!titleLine.Contains("(") && !titleLine.Contains(")")) return @else;
            if (!IsTitle(titleLine)) return @else;
            else return titleLine.Between("(", ")");
        }
        //public static DocParagraph GetParagraphFromPath(string path, IEnumerable<DocParagraph> paragraphs, DocSearchType searchType = DocSearchType.ID)
        //{
        //    path = CleanParagraphPath(path);
        //    if(path == "0" || path.Length == 0 || path.EndsWith("\\")) throw new ArgumentException("Invalid Path; " + path);
        //    string abpath = path;
        //    string pathSplitter = "\\";
        //    if (paragraphs.Count() == 0) throw new ArgumentException("Invalid IEnumerable; no values.");
        //    int currentlvl = 0;
        //    DocParagraph searching = null;
        //    int __id = 0;
        //    if (path.EndsWith(pathSplitter)) path = path[..pathSplitter.Length];
        //    if (searchType == DocSearchType.ID)
        //        foreach (string _id in path.Split(pathSplitter))
        //        {
        //            currentlvl++;
        //            if (currentlvl == 1 && paragraphs.Where(p => p.Level == currentlvl && p.ID == int.Parse(_id)).Count() == 1) //Paraghaps exist
        //                searching = paragraphs.Where(p => p.Level == currentlvl && p.ID == int.Parse(_id)).First();
        //            else if (searching.Subs.Where(p => p.Level == currentlvl && p.ID == int.Parse(_id)).Count() == 1)
        //                searching = searching.Subs.Where(p => p.Level == currentlvl && p.ID == int.Parse(_id)).First();
        //            else throw new ArgumentException("Invalid Sub Path.");
        //        }
        //    else if (searchType == DocSearchType.Title)
        //        foreach (string _name in path.Split(pathSplitter))
        //        {
        //            currentlvl++;
        //            if (currentlvl == 1 && paragraphs.Where(p => p.Level == currentlvl && p.Title == _name).Count() == 1) //Paraghaps exist
        //                searching = paragraphs.Where(p => p.Level == currentlvl && p.Title == _name).First();
        //            else if (searching.Subs.Where(p => p.Level == currentlvl && p.Title == _name).Count() == 1)
        //                searching = searching.Subs.Where(p => p.Level == currentlvl && p.Title == _name).First();
        //            else throw new ArgumentException("Invalid Sub Path.");
        //        }
        //    else
        //        foreach (string _nameOrID in path.Split(pathSplitter))
        //        {
        //            currentlvl++;
        //            __id = _nameOrID.SafeParse();
        //            if (__id == -1)
        //            {
        //                if (currentlvl == 1 && paragraphs.Where(p => p.Level == currentlvl && p.Title == _nameOrID).Count() == 1) //Paraghaps exist
        //                    searching = paragraphs.Where(p => p.Level == currentlvl && p.Title == _nameOrID).First();
        //                else if (searching.Subs.Where(p => p.Level == currentlvl && p.Title == _nameOrID).Count() == 1)
        //                    searching = searching.Subs.Where(p => p.Level == currentlvl && p.Title == _nameOrID).First();
        //                else throw new ArgumentException("Invalid Sub Path.");
        //            }
        //            else
        //            {
        //                if (currentlvl == 1 && paragraphs.Where(p => p.Level == currentlvl && p.ID == __id).Count() == 1) //Paraghaps exist
        //                    searching = paragraphs.Where(p => p.Level == currentlvl && p.ID == __id).First();
        //                else if (searching.Subs.Where(p => p.Level == currentlvl && p.ID == __id).Count() == 1)
        //                    searching = searching.Subs.Where(p => p.Level == currentlvl && p.ID == __id).First();
        //                else throw new ArgumentException("Invalid Sub Path.");
        //            }
        //        }
        //    //Console.WriteLine(abpath + "<--");
        //    //Console.WriteLine(Regex.Matches(abpath.Replace("\\", "<splt>"), "<splt>").Count + ", " + searching.Level);

        //    //if(Regex.Matches(abpath.Replace("\\", "<splt>"), "<splt>").Count != searching.Level) throw new ArgumentException("Invalid Sub Path.");
        //    Console.WriteLine("returned; " + searching.IDPath);
        //    return searching;
        //}
        public static ParagraphType GetParagraphFromPath<ParagraphType>(string path, IEnumerable<ParagraphType> paragraphs) where ParagraphType : IDocParagraph
        {
            if (paragraphs.Count() == 0) throw new ArgumentException("Invalid IEnumerable; no values.");
            try
            {
                return paragraphs.Where(p => p.Path == path || p.Path == path).Last();
            }
            catch (InvalidOperationException)
            {
                throw new ArgumentException("Invalid Path.");
            }
        }
        /// <summary>
        /// Convert a integer to a <see cref="string"/> object consisting of ':' and '.'.
        /// </summary>
        /// <param name="i">Integer to convert.</param>
        /// <returns>A string consisting of ':' and '.' representing the specified integer.</returns>
        public static string IntToPunc(int i)
        {
            string toreturn = i < 0 ? "-" : string.Empty;
            for (i = System.Math.Abs(i); i != 1 && i != 0; i -= 2) toreturn += ':';
            return i == 1 ? (toreturn + '.') : toreturn;
        }
        public static bool IsTitle(string line, int lineNum = 0)
            => lineNum == 0 && line.StartsWith('[') && 
            line[1] != '.' && line[1] != ':' && 
            line.Where(c => c == '[').Count() == 1 && line.Where(c => c == ']').Count() == 1 && 
            (line.EndsWith(']') || (line.EndsWith(')') && line.Where(c => c == '(').Count() == 1) && 
            line.Where(c => c == ')').Count() == 1 && line.Contains("]("));
        public static bool IsHeader(string line) //to refine (to regex)
            => line.StartsWith("[.") || line.StartsWith("[:") && (line.EndsWith("]") ||
                    line.EndsWith(")") && line.Contains("](") && line.Where(c => c.ToString() == "(").Count() == 1) &&
                    line.Where(c => c.ToString() != string.Empty).Count() != 3 &&
                    line.Where(c => c == '[' || c == ']').Count() < 3;
        public static bool IsHeader(string line, int depthToSatisfy)
           => IsHeader(line) && new Header(line).Depth == depthToSatisfy;
        /// <summary>
        /// Convert Dot string to Int32.
        /// </summary>
        /// <param name="str">String to convert.</param>
        /// <returns>A Integer representing this string. (Dot count; '.' and ':')</returns>
        public static int DepthToString(string str)
        {
            int toreturn = 0;
            foreach (char c in str)
                if (c == '.') toreturn++;
                else if (c == ':') toreturn += 2;
            if (str.StartsWith('-')) return toreturn * -1;
            return toreturn;
        }
        /// <summary>
        /// Gets the keys and statements from a string.
        /// </summary>
        /// <param name="str">String to get keys and statements from.</param>
        /// <returns>Keys and Statements of the string.</returns>
        /// <exception cref="ArgumentException"></exception>
        public static (IEnumerable<KeyType> Keys, IEnumerable<StatementType> Statements) GetKeysAndStatements<KeyType, StatementType>(string str)
            where KeyType : DocKeyBase, new() where StatementType : DocStatementBase, new()
            => GetKeysAndStatements<KeyType, StatementType>(new StreamReader(str.ToStream()));
        /// <summary>
        /// Gets the keys and statements from a byte array given a encoding.
        /// </summary>
        /// <param name="bytes">Byte array representing a string.</param>
        /// <param name="e"></param>
        /// <returns>Keys and Statements of the string.</returns>
        public static (IEnumerable<KeyType> Keys, IEnumerable<StatementType> Statements) GetKeysAndStatements<KeyType, StatementType>(byte[] bytes, Encoding e)
            where KeyType : DocKeyBase, new() where StatementType : DocStatementBase, new()
            => GetKeysAndStatements<KeyType, StatementType>(e.GetString(bytes));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="thisReader"></param>
        /// <param name="dispose"></param>
        /// <returns>Keys and Statements of the string.</returns>
        /// <exception cref="ArgumentException"></exception>
        public static (IEnumerable<KeyType> Keys, IEnumerable<StatementType> Statements) GetKeysAndStatements<KeyType, StatementType>(StreamReader thisReader, bool dispose = false)
            where KeyType : DocKeyBase, new() where StatementType : DocStatementBase, new()
        {
            int mempos = (int)thisReader.BaseStream.Position;
            if (thisReader.BaseStream.Length == 0) throw new ArgumentException("Can't Retrieve Keys from Uninitialized Content.");
            List<KeyType> retTextKeys = new List<KeyType>();
            List<StatementType> retTextStatements = new List<StatementType>();
            KeyType key = new KeyType();
            string _valueChecker = string.Empty;
            bool readingKeyStart = false;
            bool readingKeyEnd = false;
            bool readingKey = false;
            bool readingValue = false;
            int signatureEnd = 0;
            string fullKey = String.Empty;
            void Reset()
            {
                fullKey = String.Empty;
                readingKeyStart = false;
                readingKeyEnd = false;
                readingKey = false;
                readingValue = false;
                key = new KeyType();
                _valueChecker = string.Empty;
                signatureEnd = 0;
            }
            for (int i = 0; i < thisReader.BaseStream.Length; i++)
            {
                char current = (char)thisReader.Read();
                switch (current)
                {
                    case ' ':
                        if (!readingValue)
                        {
                            readingKeyEnd = false;
                            readingKey = false;
                            readingKeyStart = false;
                        }
                        break;
                    case '<':
                        if (!readingKey) readingKeyStart = true;
                        else readingKeyEnd = true;
                        if (readingKeyEnd) readingValue = false;
                        readingKey = true;
                        break;
                    case '>':
                        if (readingKeyStart)
                        {
                            signatureEnd = i;
                            if (fullKey.StartsWith("<//"))
                            {
                                fullKey += '>';
                                key.Key = key.Key[3..];
                                key.Range = (i + 1 - fullKey.Length)..(i + 1 - fullKey.Length + fullKey.Length);
                                if (key.Key.GetCharCount("::") == 1)
                                {
                                    key.SecondaryKey = key.Key.Split("::").Last();
                                    key.Key = key.Key.Split("::").First();
                                }
                                retTextStatements.Add(key.ToStatement<StatementType>());
                                Reset();
                                break;
                            }
                        }
                        if (readingKeyEnd)
                        {
                            fullKey += '>';
                            key.Value = key.Value[1..];
                            if (fullKey.Length < 6)
                            {
                                Reset();
                                break;
                            }
                            if (key.Key.GetCharCount("::") == 1)
                            {
                                key.SecondaryKey = key.Key.Split("::").Last();
                                key.Key = key.Key.Split("::").First();
                                key.Key = key.Key[1..];
                            }
                            else if (key.Key.GetCharCount("::") != 1 && !key.Key.StartsWith("<//")) key.Key = key.Key[1..];
                            if (key.Key != _valueChecker[2..])
                            {
                                //Console.WriteLine(key.FullKey);
                                //Console.WriteLine(key.Key);
                                //Console.WriteLine(_valueChecker[2..] + "-< 2");
                                thisReader.BaseStream.Position = mempos;
                                for (int i2 = 0; i2 < signatureEnd + 1; i2++) thisReader.Read();
                                i = signatureEnd;
                                Reset();
                                break;
                            }
                            if (!_valueChecker.StartsWith("</") || _valueChecker[2..] != key.Key)
                            {
                                Reset();
                                break;
                            }
                            //if (key.SecondaryKey == "") key.Range = (key.OnChar + ("<" + key.Key + ">").Length)..(key.OnChar + ("<" + key.Key + ">" + key.Value).Length);
                            //else key.Range = (key.OnChar + ("<" + key.Key + "::" + key.SecondaryKey + ">").Length)..(key.OnChar + ("<" + key.Key + "::" + key.SecondaryKey + ">" + key.Value).Length);
                            key.Range = (i + 1 - fullKey.Length)..(i + 1 - fullKey.Length + fullKey.Length);

                            retTextKeys.Add(key);
                            Reset();
                        }
                        else
                        {
                            readingValue = true;
                            readingKeyStart = false;
                        }
                        break;
                }
                if (readingKey)
                {
                    fullKey += current;
                    if (readingKeyStart) key.Key += current;
                    if (readingKeyEnd) _valueChecker += current;
                    if (readingValue) key.Value += current;
                }
            }
            thisReader.BaseStream.Position = mempos;
            if (dispose) thisReader.Dispose();
            return (retTextKeys, retTextStatements);
        }
    }
    [Serializable]
    internal class InvalidKeyException : Exception
    {
        public InvalidKeyException() { }
        public InvalidKeyException(string message) : base(message) { }
        public InvalidKeyException(string message, Exception innerException) : base(message, innerException) { }
        protected InvalidKeyException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}

//public static (IEnumerable<DocKey> Keys, IEnumerable<DocStatement> Statements) GetKeysAndStatements(StreamReader th)
//{

//    if (thisReader. == string.Empty) throw new ArgumentException("Can't Retrieve Keys from Uninitialized Content.");
//    List<DocKey> retTextKeys = new List<DocKey>();
//    List<DocStatement> retTextStatements = new List<DocStatement>();
//    DocKey key = new DocKey();
//    string _valueChecker = string.Empty;
//    bool readingKeyStart = false;
//    bool readingKeyEnd = false;
//    bool readingKey = false;
//    bool readingValue = false;
//    int signatureEnd = 0;
//    string fullKey = String.Empty;
//    void Reset()
//    {
//        fullKey = String.Empty;
//        readingKeyStart = false;
//        readingKeyEnd = false;
//        readingKey = false;
//        readingValue = false;
//        key = new DocKey();
//        _valueChecker = string.Empty;
//        signatureEnd = 0;
//    }
//    for (int i = 0; i < str.Length; i++)
//    {
//        char current = (char)thisReader.Read();
//        switch (current)
//        {
//            case ' ':
//                if (!readingValue)
//                {
//                    readingKeyEnd = false;
//                    readingKey = false;
//                    readingKeyStart = false;
//                }
//                break;
//            case '<':
//                if (!readingKey) readingKeyStart = true;
//                else readingKeyEnd = true;
//                if (readingKeyEnd) readingValue = false;
//                readingKey = true;
//                break;
//            case '>':
//                if (readingKeyStart)
//                {
//                    signatureEnd = i;
//                    if (fullKey.StartsWith("<//"))
//                    {
//                        fullKey += '>';
//                        key.Key = key.Key[3..];
//                        key.Range = (i + 1 - fullKey.Length)..(i + 1 - fullKey.Length + fullKey.Length);
//                        if (key.Key.GetCharCount("::") == 1)
//                        {
//                            key.SecondaryKey = key.Key.Split("::").Last();
//                            key.Key = key.Key.Split("::").First();
//                        }
//                        retTextStatements.Add(key.ToStatement());
//                        Reset();
//                        break;
//                    }
//                }
//                if (readingKeyEnd)
//                {
//                    fullKey += '>';
//                    key.Value = key.Value[1..];
//                    if (fullKey.Length < 6)
//                    {
//                        Reset();
//                        break;
//                    }
//                    if (key.Key.GetCharCount("::") == 1)
//                    {
//                        key.SecondaryKey = key.Key.Split("::").Last();
//                        key.Key = key.Key.Split("::").First();
//                        key.Key = key.Key[1..];
//                    }
//                    else if (key.Key.GetCharCount("::") != 1 && !key.Key.StartsWith("<//")) key.Key = key.Key[1..];
//                    if (key.Key != _valueChecker[2..])
//                    {
//                        //Console.WriteLine(key.FullKey);
//                        //Console.WriteLine(key.Key);
//                        //Console.WriteLine(_valueChecker[2..] + "-< 2");
//                        thisReader = new StringReader(str);
//                        for (int i2 = 0; i2 < signatureEnd + 1; i2++) thisReader.Read();
//                        i = signatureEnd;
//                        Reset();
//                        break;
//                    }
//                    if (!_valueChecker.StartsWith("</") || _valueChecker[2..] != key.Key)
//                    {
//                        Reset();
//                        break;
//                    }
//                    Console.WriteLine(key.Key + " add");
//                    //if (key.SecondaryKey == "") key.Range = (key.OnChar + ("<" + key.Key + ">").Length)..(key.OnChar + ("<" + key.Key + ">" + key.Value).Length);
//                    //else key.Range = (key.OnChar + ("<" + key.Key + "::" + key.SecondaryKey + ">").Length)..(key.OnChar + ("<" + key.Key + "::" + key.SecondaryKey + ">" + key.Value).Length);
//                    key.Range = (i + 1 - fullKey.Length)..(i + 1 - fullKey.Length + fullKey.Length);

//                    retTextKeys.Add(key);
//                    Reset();
//                }
//                else
//                {
//                    readingValue = true;
//                    readingKeyStart = false;
//                }
//                break;
//        }
//        if (readingKey)
//        {
//            fullKey += current;
//            if (readingKeyStart) key.Key += current;
//            if (readingKeyEnd) _valueChecker += current;
//            if (readingValue) key.Value += current;
//        }
//    }
//    thisReader.Dispose();
//    return (retTextKeys, retTextStatements);
//}
//    }