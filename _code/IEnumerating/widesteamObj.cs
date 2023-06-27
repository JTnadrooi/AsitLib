//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Diagnostics.CodeAnalysis;
//using System.IO;
//using System.Linq;
//using System.Text;

//namespace AsitLib.Widestream
//{
//    public class WideStreamEnumerator<TStream> : IEnumerator<string> where TStream : Stream
//    {
//        private readonly WideStream<TStream> _source;
//        public string Current => throw new NotImplementedException();
//        object IEnumerator.Current => Current;

//        public WideStreamEnumerator(WideStream<TStream> source)
//        {
//            _source = source;
//        }

//        public void Dispose() => _source.Dispose();

//        public bool MoveNext()
//        {
//            throw new NotImplementedException();
//        }

//        public void Reset()
//        {
//            throw new NotImplementedException();
//        }

//    }
//    /// <summary>
//    /// Initialize a <see cref="Stream"/> with a larger view area than one byte, the underlying technique of almost all the classes from <see cref="AsitLib.IO"/>.
//    /// <i>(ThiccStream sounded to unprofesional.)</i>
//    /// </summary>
//    /// <typeparam name="TStream"></typeparam>
//    public class WideStream<TStream> : IEnumerable<string>, IDisposable where TStream : Stream
//    {
//        public TStream Base { get; }
//        public Encoding Encoding { get; }
//        public WideStream(TStream @base, Encoding e)
//        {
//            Base = @base;
//        }
//        public WideStream(TStream @base) : this(@base, Encoding.UTF8) { }
//        public WideStream(string source) : this((TStream)source.ToStream()) { }
//        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
//        public IEnumerator<string> GetEnumerator() => new WideStreamEnumerator<TStream>(this);
//        public void Dispose()
//        {
//            throw new NotImplementedException();
//        }
//        public string Next()
//        {

//        }
//        public void Next(byte[] bytes, int startIndex, int endIndex) => AsitLibStatic.Fill(bytes, Next(), startIndex, endIndex, Encoding);
//    }
//    /// <summary>
//    /// Read a <see cref="Stream"/> in chunks.
//    /// <i>(ChunkyStream sounded to unprofesional.)</i>
//    /// </summary>
//    /// <typeparam name="TStream"></typeparam>
//    public class ChunkStream<TStream> : IEnumerable<string>, IDisposable where TStream : Stream
//    {
//        public void Dispose()
//        {
//            throw new NotImplementedException();
//        }

//        public IEnumerator<string> GetEnumerator()
//        {
//            throw new NotImplementedException();
//        }

//        IEnumerator IEnumerable.GetEnumerator()
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
