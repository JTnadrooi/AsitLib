using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Linq;
#nullable enable

namespace AsitLib.Collections
{
    public class ChunkEnumerableEnumerator<T> : IEnumerator<T[]>
    {
        public ChunkEnumerableEnumerator(ChunkEnumerable<T> source)
        {
            this.source = source;
        }
        private bool pastEnd = false;
        private readonly ChunkEnumerable<T> source;
        private T[]? current;
        public T[] Current
        {
            get
            {
                if (pastEnd) throw new InvalidOperationException("Enumeration already finished.");
                if (current == null) throw new InvalidOperationException("Enumeration has not started. Call MoveNext.");
                return current;
            }
        }
        object IEnumerator.Current => Current;
        public void Dispose() => source.Dispose();
        public void Reset()
        {
            current = new T[0];
            source.Reset();
        }
        public bool MoveNext()
        {
            var t = source.Next();
            current = t.returned;
            pastEnd = !t.succes;
            return t.succes;
        }
    }
    public class ChunkEnumerable<T> : IEnumerable<T[]>, IDisposable
    {
        private int index;
        public ReadOnlyCollection<T> MostRecent => viewArray.Copy().AsReadOnly();
        private T[] viewArray;
        private IEnumerator<T> sourceEnumerator;
        /// <summary>
        /// The view width.
        /// </summary>
        public int Width => viewArray.Length;
        /// <summary>
        /// The current index.
        /// </summary>
        public int Index => index;
        /// <summary>
        /// The source <see cref="IEnumerable{T}"/>.
        /// </summary>
        public IEnumerable<T> Source { get; }
        /// <summary>
        /// Contruct a new <see cref="ChunkEnumerable{T}"/> from the specified <see cref="IEnumerable{T}"/> <paramref name="source"/>.
        /// </summary>
        /// <param name="source">The <see cref="Source"/> <see cref="IEnumerable{T}"/>.</param>
        /// <param name="width">The split width.</param>
        public ChunkEnumerable(IEnumerable<T> source, int width)
        {
            sourceEnumerator = source.GetEnumerator();
            viewArray = new T[width];
            Source = source;
        }
        /// <summary>
        /// Get the next <see cref="Array"/> in the view area, advansing the <see cref="Index"/> by the <see cref="Width"/>.
        /// </summary>
        /// <returns> 
        /// A <see cref="Tuple{T1, T2, T3}"/> containing a <see cref="Boolean"/>
        /// indicating if the operation was a succes, the <see cref="Array"/> in the view area and the current <see cref="Index"/>.
        /// </returns>
        public (bool succes, T[] returned, int index) Next()
        {
            T[] temp = new T[viewArray.Length];
            Array.Copy(viewArray, temp, temp.Length);
            int added = 0;
            viewArray = new T[Width];
            for (int i = 0; i < viewArray.Length; i++)
            {
                if (!sourceEnumerator.MoveNext())
                    if (added == 0) return (false, viewArray, index);
                    else break;
                viewArray[i] = sourceEnumerator.Current;
                added++;
            }
            index += added;
            return (true, viewArray.Copy(), index);
        }
        public IEnumerator<T[]> GetEnumerator() => new ChunkEnumerableEnumerator<T>(this);
        IEnumerator IEnumerable.GetEnumerator() => (IEnumerator)new ChunkEnumerableEnumerator<T>(this);
        public void Dispose() { }
        /// <summary>
        /// Reset this <see cref="ChunkEnumerable{T}"/> to its initial position, before the first element.
        /// </summary>
        public void Reset()
        {
            viewArray = new T[Width];
            sourceEnumerator.Reset();
            index = -1;
        }
        public T[,] To2DArray()
            => this.ToArray().CreateRectangularArray();
    }
    public static class ChunkEnumerable
    {

    }
}
