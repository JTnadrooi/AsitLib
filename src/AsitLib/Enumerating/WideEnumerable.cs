using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Collections.ObjectModel;
#nullable enable

namespace AsitLib.Collections
{
    /// <summary>
    /// The <see cref="IEnumerator{T}"/> used for <see cref="WideEnumerable{T}"/> objects.
    /// </summary>
    /// <typeparam name="T">The containing <see cref="Array"/> return type.</typeparam>
    public class WideEnumerableEnumerator<T> : IEnumerator<T[]>
    {
        private bool pastEnd = false;
        private readonly WideEnumerable<T> source;
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
            current = t;
            pastEnd = t != null;
            return t != null;
        }
        public WideEnumerableEnumerator(WideEnumerable<T> source)
        {
            current = null;
            this.source = source;
            source.Reset();
        }
    }
    /// <summary>
    /// A efficient way of viewing a <see cref="IEnumerable{T}"/> in even <see cref="Array"/> objects. <br/>
    /// <strong>Elements may appear double. If this is unwanted, see; </strong>
    /// </summary>
    /// <typeparam name="T">The containing <see cref="Array"/> return type of the <see cref="Next"/> method.</typeparam>
    public class WideEnumerable<T> : IDisposable, IEnumerable<T[]>
    {
        private int index;
        public ReadOnlyCollection<T> MostRecent => Array.AsReadOnly(viewArray.Copy());
        private T[] viewArray;
        private IEnumerator<T> sourceEnumerator;
        private bool prevSucces = true;
        

        /// <summary>
        /// The view width.
        /// </summary>
        public int Width { get; }
        /// <summary>
        /// The current index.
        /// </summary>
        public int Index => index;
        /// <summary>
        /// The source <see cref="IEnumerable{T}"/>.
        /// </summary>
        public IEnumerable<T> Source { get; }
        /// <summary>
        /// Contruct a new <see cref="WideEnumerable{T}"/> with set width.
        /// </summary>
        /// <param name="source">The <see cref="Source"/> <see cref="IEnumerable{T}"/>.</param>
        /// <param name="width">The view <see cref="Width"/>.</param>
        public WideEnumerable(IEnumerable<T> source, int width)
        {
            Width = width;
            Source = source;
            index = 0;
            sourceEnumerator = Source.GetEnumerator();
            //sourceEnumerator.Reset();
            viewArray = new T[Width];
        }
        public void Dispose() { }
        /// <summary>
        /// Get the next <see cref="Array"/> in the view area, advansing the <see cref="Index"/> by 1.
        /// </summary>
        /// <returns>
        /// A <see cref="Tuple{T1, T2, T3}"/> containing a <see cref="Boolean"/>
        /// indicating if the operation was a succes, the <see cref="Array"/> in the view area and the current <see cref="Index"/>.
        /// </returns>
        public T[]? Next()
        {         
            bool succes = sourceEnumerator.MoveNext(); // check if movenext is valid.
            prevSucces = succes; //?
            if (!succes) // if not succes, return stuff
            {
                return null;
            }

            for (int i = 0; i < viewArray.Length; i++)
            {
                if (viewArray.Length >= i + 2)  
                    viewArray[i] = viewArray[i + 1];
                else
                {
                    viewArray[i] = sourceEnumerator.Current;
                    //Console.WriteLine("b");
                }
            }
            //Console.WriteLine(viewArray.ToJoinedString() + "--" + succes);

            index++;
            return viewArray.Copy();
        }
        /// <summary>
        /// Reset this <see cref="WideEnumerable{T}"/> to its initial position, before the first element.
        /// </summary>
        public void Reset()
        {
            viewArray = new T[Width];
            sourceEnumerator.Reset();
            index = -1;
        }
        public IEnumerator<T[]> GetEnumerator() => new WideEnumerableEnumerator<T>(this);
        IEnumerator IEnumerable.GetEnumerator() => (IEnumerator)new WideEnumerableEnumerator<T>(this);
    }
    public static class WideEnumerable
    {

    }
}
