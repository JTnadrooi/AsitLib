using AsitLib;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;

namespace AsitLib.IO
{
    /// <summary>
    /// A class that implements <see cref="IEnumerator{T}"/>. This is for the <see cref="AsitStreamFile"/> class.
    /// </summary>
    public class AsitDataEnumerator : IEnumerator<string>
    {
        private AsitStreamFile _stream;
        private string _current;
        public string Current
        {
            get
            {
                if (_current == null) throw new InvalidOperationException();
                else return _current;
            }
        }
        object IEnumerator.Current => Current;
        internal AsitDataEnumerator(AsitStreamFile stream) => _stream = stream;
        public bool MoveNext()
        {
            _current = _stream.Next();
            if (_current == null)
            {
                _stream.SetPosition(0);
                return false;
            }
            else return true;
        }
        public void Reset() => _stream.SetPosition(0);
        public void Dispose() { }
    }
    /// <summary>
    /// Fast and optimized way of storing <see cref="Array"/> objects.
    /// </summary>
    public class AsitStreamFile : IEnumerable<string>, IDisposable, IAsitFile
    {
        /// <summary>
        /// Path of the document with the stored <see cref="Array"/>.
        /// </summary>
        public string Path { get; internal set; }
        private FileStream _stream;
        /// <summary>
        /// <see cref="FileStream"/> used for this <see cref="AsitStreamFile"/>.
        /// </summary>
        public FileStream Stream { get { return _stream; } }
        /// <summary>
        /// Gets the lenght of the stored <see cref="Array"/> without buffering.
        /// </summary>
        public int DataCount { get; internal set; }
        /// <summary>
        /// Gets if this object is initialized. ( -\updated) 
        /// </summary>
        public bool Initialized { get; internal set; }
        /// <summary>
        /// Size of the document <strong>Not this object</strong>.
        /// </summary>
        public long Size => _stream.Length;
        /// <summary>
        /// Lenght of the lenght of the buffer.
        /// </summary>
        public int BufferLength { get; internal set; }
        /// <summary>
        /// Current position.
        /// </summary>
        public int ReadPosition { get; internal set; }
        /// <summary>
        /// Lenght of the buffer.
        /// </summary>
        public int Buffer { get; internal set; }
        /// <summary>
        /// Gets if this <see cref="AsitStreamFile"/> has been <see langword="closed"/>.
        /// </summary>
        public bool IsClosed { get; internal set; }

        public Encoding Encoding => Encoding.ASCII;

        public string FileType => "asitStreamFile";

        public int Version => 2;

        public string CoreExtension => ".asits";

        public object[] Other { get; internal set; }

        public FileInfo FileInfo => new FileInfo(this.Path);

        /// <summary>
        /// Gets a <see cref="Array"/> of <see cref="string"/> objects at the specified <see cref="Range"/>.
        /// </summary>
        /// <param name="range"><see cref="Range"/> to get the <see cref="string"/> objects from.</param>
        /// <returns>The <see cref="Array"/> of <see cref="string"/> objects in the specified <see cref="Range"/>.</returns>
        public string[] this[Range range] => GetData(range);
        /// <summary>
        /// Gets <see cref="string"/> at the specified <paramref name="index"/>
        /// </summary>
        /// <param name="index"><see cref="Index"/> to get data from.</param>
        /// <returns>The <see cref="string"/> at the specified <paramref name="index"/>.</returns>
        public string this[Index index] => GetData(index);
        /// <summary>
        /// Point to a file that effectively holds an <see cref="string"/> <see cref="Array"/>.
        /// </summary>
        /// <param name="path">Path to the file.</param>
        /// <exception cref="ArgumentException"></exception>
        public AsitStreamFile(string path) : this(new FileInfo(path)) { }
        /// <summary>
        /// Point to a file that effectively holds an <see cref="string"/> <see cref="Array"/>.
        /// </summary>
        /// <param name="file"><see cref="FileInfo"/> of the file.</param>
        /// <exception cref="ArgumentException"></exception>
        public AsitStreamFile(FileInfo file)
        {
            //do checks
        }
        public AsitStreamFile(FileInfo file, int bufferSize, int arrayLenght)
        {
            //asign all, no long checks
            Path = file.FullName;
            DataCount = arrayLenght;
            Initialized = true;
            ReadPosition = 0;
            Buffer = bufferSize;
            BufferLength = bufferSize.ToString().Length;
            IsClosed = false;
            _stream = new FileStream(file.FullName, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        }
        ///// <summary>
        ///// Point to a file that effectively holds an <see cref="string"/> <see cref="Array"/>.
        ///// </summary>
        ///// <param name="strArr"><see cref="Array"/> of <see cref="string"/> objects.</param>
        ///// <param name="path">Path to the file that gets created.</param>
        ///// <param name="buffer">Lenght of the <see cref="Buffer"/>. <strong>Can't be shorter than the shortest value in the given <see cref="Array"/>.</strong></param>
        //public AsitDataFile(IEnumerable<string> strArr, string path, int buffer)
        //{
        //    if (buffer == 0) buffer = strArr.Min(s => s.Length);
        //    Console.WriteLine("bb");



        //    Setup(new FileInfo(path));
        //}
        //private void Setup(FileInfo file)
        //{
        //    ReadPosition = 0;
        //    string path = file.FullName;
        //    if (!File.Exists(path)) throw new ArgumentException("Path doesn't exist.");
        //    Path = path;
        //    _stream = File.Open(path, FileMode.Open);
        //    for (int i = 0; i < 1 + BufferLength; i++) if (((char)_stream.ReadByte()).ToString().SafeIntParse() == -1)
        //            throw new ArgumentException("Invalid Data File");
        //    _stream.Position = 0;
        //    BufferLength = int.Parse(Encoding.ASCII.GetString(new byte[] { (byte)_stream.ReadByte() }));
        //    string bufferstr = string.Empty;
        //    for (int i = 0; i < BufferLength; i++) bufferstr += (char)_stream.ReadByte();
        //    Buffer = int.Parse(bufferstr);
        //    _stream.Position = 0;
        //    Initialized = true;
        //}
        /// <summary>
        /// Dispose the containing <see cref="FileStream"/>. (<see cref="Stream"/>
        /// </summary>
        public void Dispose()
        {
            Initialized = false;
            _stream.Dispose();
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Gets <see cref="string"/> at the specified <paramref name="index"/>
        /// </summary>
        /// <param name="index"><see cref="Index"/> to get data from.</param>
        /// <returns>The <see cref="string"/> at the specified <paramref name="index"/>.</returns>
        /// <exception cref="ArgumentException"></exception>
        public string GetData(Index index)
        {
            //Set postion.
            SetPosition(0);

            //get index
            uint iIndex = (uint)index.Value;

            //initialize buffer and reader.
            byte[] bufferData = new byte[Buffer];
            using BinaryReader reader = new BinaryReader(_stream, Encoding.ASCII, true);

            //why
            if (index.IsFromEnd) reader.BaseStream.Position = 1 + BufferLength + (DataCount * (Buffer + 1)) - (index.Value * (Buffer + 1));
            else reader.BaseStream.Position = 1 + BufferLength + iIndex * Buffer + iIndex;
            //1+


            //Fill
            reader.Read(bufferData, 0, bufferData.Length);

            //
            string toReturn = Encoding.ASCII.GetString(bufferData).Replace(AsitDataUtils.Filler.ToString(), "");
            _stream.Position = 0;
            
            //when does this even trigger????
            if (toReturn == string.Empty) throw new ArgumentException("Invalid Index: " + index);
            else return toReturn;
        }
        /// <summary>
        /// Gets a <see cref="Array"/> of <see cref="string"/> objects at the specified <see cref="Range"/>.
        /// </summary>
        /// <param name="range"><see cref="Range"/> to get the <see cref="string"/> objects from.</param>
        /// <returns>The <see cref="Array"/> of <see cref="string"/> objects in the specified <see cref="Range"/>.</returns>
        public string[] GetData(Range range)
        {
            SetPosition(range.Start.Value);
            List<string> toreturn = new List<string>();
            int l = range.GetOffsetAndLength(this.DataCount).Length;
            for (int i = 0; i < l; i++)
            {
                string ths = Next();
                toreturn.Add(ths);
            }
            return toreturn.ToArray();
        }
        /// <summary>
        /// Get this <see cref="AsitStreamFile"/> as a <see cref="IEnumerable"/>.
        /// </summary>
        /// <returns>This <see cref="AsitStreamFile"/> as a <see cref="IEnumerable"/>.</returns>
        public IEnumerable<string> GetFull()
        {
            int mempos = this.ReadPosition;
            SetPosition(0);
            List<string> toReturn = new List<string>();
            byte[] bufferData = new byte[Buffer + 1];
            for (; _stream.Read(bufferData, 0, bufferData.Length) > 0;)
                toReturn.Add(Encoding.ASCII.GetString(bufferData[..Buffer]));
            SetPosition(mempos);
            return toReturn;
        }
        /// <summary>
        /// Append a <see cref="string"/> to this <see cref="AsitStreamFile"/>. <br/>
        /// <i>This only works if the max lenght of the largest <see cref="string"/> in the <paramref name="strArr"/> is shorter or equal to the
        /// lenght of the largest <see cref="string"/> in the <see cref="AsitStreamFile"/>. <br/>
        /// If this isn't the case, a <see cref="ArgumentException"/> is thrown.</i>
        /// </summary>
        /// <param name="strArr"><see cref="Array"/> of <see cref="string"/> objects.</param>
        /// <exception cref="ArgumentException">New Buffer is bigger than old Buffer.</exception>
        public void Append(IEnumerable<string> strArr)
        {
            int mempos = this.ReadPosition;
            (int arrBuffer, int arrBufferLenght, int arrCount) tup = AsitDataUtils.GetAsitData(strArr);
            if (tup.arrBuffer > Buffer) throw new ArgumentException("The IEnumerable has a larger buffer than this AsitDataStream.");
            _stream.Position = _stream.Length;
            foreach(string str in strArr)
            {
                _stream.WriteByte((byte)AsitDataUtils.Splitter);
                for (int i = 0; i < Buffer - str.Length; i++) _stream.Write(Encoding.ASCII.GetBytes(AsitDataUtils.Filler.ToString()));
                _stream.Write(Encoding.ASCII.GetBytes(str));
                _stream.Flush();
            }
            DataCount += tup.arrCount;
            SetPosition(mempos);
        }
        /// <summary>
        /// Set a new buffer.<br/><strong>Rewites the entire file.</strong>
        /// </summary>
        /// <param name="buffer">New buffer.</param>
        /// <exception cref="InvalidOperationException"></exception>
        public void SetBuffer(int buffer)
        {
            int mempos = ReadPosition;
            int minbuffer = this.ToArray().Select(s => s.Length).Max();
            if (buffer < minbuffer) throw new InvalidOperationException("Buffer to small.");
            _stream.Position = 0;
            byte[] bytes = new byte[_stream.Length];
            _stream.Read(bytes);
            string content = Encoding.ASCII.GetString(bytes);
            content = content[(1 + BufferLength)..];
            Buffer = buffer;
            BufferLength = buffer.ToString().Length;
            _stream.SetLength(0);
            _stream.Position = 0;
            _stream.Write(Encoding.ASCII.GetBytes(BufferLength.ToString() + buffer.ToString()));
            foreach (string str in content.Split(AsitDataUtils.Splitter))
            {
                _stream.WriteByte((byte)AsitDataUtils.Splitter);
                for (int i = 0; i < Buffer - str.Length; i++) _stream.Write(Encoding.ASCII.GetBytes(AsitDataUtils.Filler.ToString()));
                _stream.Write(Encoding.ASCII.GetBytes(str));
                _stream.Flush();
            }
            SetPosition(mempos);
        }
        /// <summary>
        /// Validate the integrity of this <see cref="AsitStreamFile"/>.
        /// </summary>
        /// <returns>A value inticating if this <see cref="AsitStreamFile"/> is valid.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool Validate()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Set the position of the reader. This method is relevant for the <see cref="Next"/> method
        /// </summary>
        /// <param name="position"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void SetPosition(int position)
        {
            if(position < 0 || position > DataCount) throw new ArgumentOutOfRangeException("Invalid Position");
            _stream.Position = position * (Buffer + 1) + BufferLength + 1;
            ReadPosition = position;
        }
        /// <summary>
        /// Reads the next <see cref="string"/> and advances the <see cref="ReadPosition"/> by one.
        /// Returns <see langword="null"/> when end of file has been reached.
        /// </summary>
        /// <returns>Read <see cref="string"/>.</returns>
        public string Next()
        {
            string toreturn = string.Empty;
            if (Stream.Position <= BufferLength + 1) this.SetPosition(0);
            for (int i = 0; i < Buffer; i++)
            {
                int i2 = _stream.ReadByte();
                if (i2 == -1) return null;
                toreturn += (char)i2;
            }
            _stream.ReadByte();
            this.ReadPosition++;
            return toreturn.Replace(AsitDataUtils.Filler.ToString(), "");
        }
        /// <summary>
        /// Get the <see cref="IEnumerator"/> of this <see cref="AsitStreamFile"/>.
        /// </summary>
        /// <returns>The <see cref="IEnumerator"/> of this <see cref="AsitStreamFile"/>.</returns>
        public IEnumerator<string> GetEnumerator() => new AsitDataEnumerator(this);
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
        public static explicit operator string[](AsitStreamFile asitstream) => asitstream.GetFull().ToArray();
    }
    /// <summary>
    /// A set of static utils for a <see cref="AsitStreamFile"/> object.
    /// </summary>
    public static class AsitDataUtils
    {
        /// <summary>
        /// Splitter to seperate the values when creating a new <see cref="AsitStreamFile"/>
        /// </summary>
        public static char Splitter = '\\'; //22
        /// <summary>
        /// Filler that is used to fill strings that are shorter than the buffer when creating a new <see cref="AsitStreamFile"/>
        /// </summary>
        public static char Filler = '+'; //\0
        /// <summary>
        /// Create a <see cref="AsitStreamFile"/> from a <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <param name="strArr"><see cref="Array"/> of <see cref="string"/> objects.</param>
        /// <param name="path">Path to the file that gets created.</param>
        /// <param name="buffer">Lenght of the <see cref="AsitStreamFile.Buffer"/>.<br/><strong>Can't be shorter than the shortest value in the <paramref name="strArr"/>.</strong></param>
        /// <returns>A new <see cref="AsitStreamFile"/> made from the specified <paramref name="strArr"/>.</returns>
        /// <exception cref="ArgumentException"></exception>
        public static AsitStreamFile Create(this IEnumerable<string> strArr, string path, int buffer = -1)
        {
            var getData = strArr.GetAsitData();
            if (buffer <= 0) buffer = getData.buffer;
            if (buffer > 999999999) throw new ArgumentException("To Long Buffer, may be autogenerated.");

            Console.WriteLine(buffer);
            Console.WriteLine(buffer.ToString().Length.ToString());
            Console.WriteLine(getData.Count);
            //Create file.
            using (FileStream fs = File.Create(path))
            {
                fs.Write(Encoding.ASCII.GetBytes(buffer.ToString().Length.ToString()));
                fs.Write(Encoding.ASCII.GetBytes(buffer.ToString()));
                foreach (string str in strArr)
                {
                    for (int i = 0; i < buffer - str.Length; i++) fs.Write(Encoding.ASCII.GetBytes(Filler.ToString()));
                    fs.Write(Encoding.ASCII.GetBytes(str));
                    //fs.WriteByte((byte)Splitter); //why
                }
                fs.SetLength(fs.Length);
                fs.Flush();
            }
            return new AsitStreamFile(new FileInfo(path), buffer, getData.Count);
        }
        /// <summary>
        /// Gets pre-defined values from an <see cref="Array"/> without creating a new <see cref="AsitStreamFile"/> from it.
        /// </summary>
        /// <param name="strArr"><see cref="Array"/> to get the data from.</param>
        /// <returns>A <see cref="ValueTuple{T1, T2, T3}"/> containing specifics of this <see cref="Array"/> as seen from the <see cref="AsitStreamFile"/>  initializers.</returns>
        public static (int buffer, int bufferLenght, int Count) GetAsitData(this IEnumerable<string> strArr)
        {
            int reqBuffer = 0;
            int count = 0;
            foreach (string str in strArr)
            {
                count++;
                reqBuffer = str.Length > reqBuffer ? str.Length : reqBuffer;
                if(str.Any(c => c > 255 || c < 32)) throw new Exception(str.ToString() + " contains non-ascii characters");
            }
            return(reqBuffer, reqBuffer.ToString().Length, count);
        }
        /// <summary>
        /// Gets pre-defined values from an <see cref="Array"/> from a <see cref="string"/>.<br/>
        /// <i>Why would one need this?</i>
        /// </summary>
        /// <param name="str"><see cref="string"/> to get the data from.</param>
        /// <returns>A <see cref="ValueTuple{T1, T2, T3}"/> containing specifics of this <see cref="Array"/> as seen from the <see cref="AsitStreamFile"/>  initializers.</returns>
        public static (int buffer, int bufferLenght, int Count) GetAsitData(this string str) => (str.Length, str.Length.ToString().Length, 1);
    }
}
