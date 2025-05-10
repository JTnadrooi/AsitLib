using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using AsitLib;
using AsitLib.Collections;
using AsitLib.Debug;
using System.Diagnostics.CodeAnalysis;
using System.Collections.ObjectModel;
using System.Collections;
#nullable enable

namespace AsitLib.Collections
{
    //Todo :
    //- Contructor Keys and Values set.
    // S(add) => P(notify)
    // P(add) => S(notify)
    // notify self to
    /// <summary>
    /// A way to create a segment in a <see cref="AsitDictionary{T}"/>. This also supports adding and clearing in only this part of the contained <see cref="AsitDictionary{T}"/>.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> the parent <see cref="AsitDictionary{T}"/> holds and enumerates.</typeparam>
    public class AsitDictionarySegment<T> : IAsitDictionary<T>
    {
        /// <summary>
        /// The <see cref="AsitDictionary{T}"/> this <see cref="AsitDictionarySegment{T}"/> references.
        /// </summary>
        public AsitDictionary<T> Parent { get; }
        /// <summary>
        /// The <see cref="Range"/> this <see cref="AsitDictionarySegment{T}"/> covers.
        /// </summary>
        public Range Range { get; }
        /// <summary>
        /// The <see cref="AsitRange"/> this <see cref="AsitDictionarySegment{T}"/> covers.
        /// </summary>
        public AsitRange AsitRange => Range.ToAsitRange(Parent.Capacity);
        public int Capacity { get; }
        public T this[string key]
        {
            get => Parent[key];
            set => Parent[key] = value;
        }
        public bool IsReadOnly => false;
        public int Space => Capacity - Count;
        public int Count => Keys.Count;
        public bool HasSpace => Space > 0;
        public ReadOnlyCollection<KeyValuePair<string, T>?> Source => new ReadOnlyCollection<KeyValuePair<string, T>?>(sourceSegment);

        public ICollection<string> Keys { get; private set; }
        public ICollection<T> Values => sourceSegment.WhereSelect(kvp => kvp == null ? (default(T)!, false) : (kvp.Value.Value, true)).ToList();

        public Action<KeyValuePair<string, T>, IAsitDictionary<T>>? OnRemove { get; set; }
        public Action<KeyValuePair<string, T>, IAsitDictionary<T>>? OnAdd { get; set; }
        public Action<IAsitDictionary<T>>? OnClear { get; set; }

        public T this[int index]
        {
            get => Get(index);
            set => Add(index, value);
        }

        private ArraySegment<KeyValuePair<string, T>?> sourceSegment;

        internal AsitDictionarySegment(AsitDictionary<T> parent, int startIndex, int endindex) : this(parent, startIndex..endindex) { }
        internal AsitDictionarySegment(AsitDictionary<T> parent, Range range)
        {
            Range = range;
            Parent = parent;
            var ol = range.GetOffsetAndLength(parent.Capacity);
            Capacity = ol.Length;
            sourceSegment = new ArraySegment<KeyValuePair<string, T>?>(parent.source, AsitRange.Start, Capacity);
            //Values = new Collection<T>(sourceSegment.WhereSelect<KeyValuePair<string, T>?, T>(kvp => kvp == null ? (default(T)!, false) : (kvp.Value.Value, true)).ToList());
            Keys = new Collection<string>(sourceSegment.WhereSelect(kvp => kvp == null ? (string.Empty, false) : (kvp.Value.Key, true)).ToList());
            //Console.WriteLine(Keys.Any(k => k == null));
            //Console.WriteLine(Values.Any(k => k == null));
            //Console.WriteLine(Keys.Count);
            //Console.WriteLine(Values.Count);
        }
        //todo: add index check
        //todo: reverse. add in parent => add in segment., impossible because this will not be subbed to the parent and thus must
        //notify
        internal void ON_ADD(int index, string key, T value)
        {
            //Console.WriteLine("SEGMENT KNOWS!");
            Keys.Add(key);
            //Console.WriteLine("Calling ONADD? :: " + (OnAdd != null));
            OnAdd?.Invoke(new KeyValuePair<string, T>(key, value), this);
        }
        internal void ON_CLEAR()
        {
            Keys.Clear();
            OnClear?.Invoke(this);
        }
        internal void ON_REMOVE(string key, T value)
        {
            Keys.Remove(key);
            OnRemove?.Invoke(new KeyValuePair<string, T>(key, value), this);
        }
        public void Clear()
        {
            for (int i = 0; i < Capacity; i++)
                sourceSegment[i] = null;
            Parent.ON_CLEAR(this);
            ON_CLEAR();
            //Parent.Count -= Count;
            //Keys.Clear();
            //Values.Clear();
            //using var e = Keys.GetEnumerator();
            //using var e2 = Parent.Keys.GetEnumerator();
            //while (e.MoveNext() && e2.MoveNext())
            //    if (e.Current == e2.Current) Parent.Keys.Remove(e.Current);
            //using var a = Values.GetEnumerator();
            //using var a2 = Parent.Values.GetEnumerator();
            //while (e.MoveNext() && e2.MoveNext())
            //    if (e.Current == e2.Current) Parent.Keys.Remove(e.Current);


        }
        public void CopyTo(KeyValuePair<string, T>[] array, int arrayIndex) => Parent.source[Range].CopyTo(array, arrayIndex);
        public bool Contains(KeyValuePair<string, T> item) => ContainsKey(item.Key);
        public bool ContainsKey(string key)
        {
            for (int i = 0; i < sourceSegment.Count; i++)
                if (sourceSegment[i] != null && sourceSegment[i]!.Value.Key == key)
                    return true;
            return false;
        }
        public IEnumerator<KeyValuePair<string, T>> GetEnumerator() => 
            Source.WhereSelect(kvp => kvp == null ? (new KeyValuePair<string, T>(), false) : (kvp!.Value, true)).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public bool Remove(KeyValuePair<string, T> item) => Remove(item.Key);
        public bool Remove(string key)
        {
            for (int i = 0; i < sourceSegment.Count; i++)
            {
                if (sourceSegment[i] == null) return false;
                if (sourceSegment[i]!.Value.Key == key)
                {
                    sourceSegment[i] = null;
                    return true;
                }
            }
            return false;
        }
        public bool Remove(int index, string key)
        {
            if (sourceSegment[index] == null) return false;
            Parent.ON_REMOVE(key, sourceSegment[index]!.Value.Value);
            ON_REMOVE(sourceSegment[index]!.Value.Key, sourceSegment[index]!.Value.Value);
            sourceSegment[index] = null;
            return true;
        }
        public bool TryGetValue(string key, [MaybeNullWhen(false)] out T value)
        {
            try
            {
                value = Get(key);
                return true;
            }
            catch
            {
                value = default;
                return false;

            }
        }
        public int GetIndex(string id)
        {
            int v = sourceSegment.GetFirstIndexWhere(x => x != null && x!.Value.Key == id);
            for (int i = 0; i < sourceSegment.Count; i++)
                if (sourceSegment[i] != null)
                    if (sourceSegment[i]!.Value.Key == id)
                        return i;
            throw new Exception();
        }
        public string GetKey(int index) => sourceSegment[index] == null ? throw new Exception() : sourceSegment[index]!.Value.Key;
        
        public void ForEach(Action<T> action) => ForEach((value, index, id) => action.Invoke(value));
        public void ForEach(Action<T, int, string> action)
        {
            for (int i = 0; i < sourceSegment.Count; i++)
            {
                if (sourceSegment[i] == null) continue;
                else action.Invoke(sourceSegment[i]!.Value.Value, i, sourceSegment[i]!.Value.Key);
            }
        }
        //O(1)
        public T Get(string key) => Get(GetIndex(key));
        //O(1)
        public T Get(int index) => sourceSegment[index]!.Value.Value;
        public T[] GetAll() => Values.ToArray();

        public void Add(KeyValuePair<string, T> item) => Add(item.Key, item.Value);
        public void Add(string key, T value)
        {
            for (int i = 0; i < sourceSegment.Count; i++)
                if (sourceSegment[i] == null)
                {
                    Add(i, key, value);
                    return;
                }
        }
        public void Add(int index, T value) => Add(index, index.ToString(), value);
        public void Add(int index, string key, T value)
        {
            if (index > Capacity) throw new Exception();
            if (sourceSegment[index] != null) throw new Exception();
            if (Keys.Contains(key)) throw new Exception();
            //if (Keys.Contains(key) && Keys.Count > 0) throw new Exception("key exists: " + key + "at" + Keys.GetFirstIndexWhere(s => s == key));
            //if (Keys.Any(k => k == key)) throw new Exception("key exists");
            //Console.WriteLine("KEY.COUNT= " + Keys.ToJoinedString(" "));
            //foreach (var item in Keys)
            //{
            //    Console.WriteLine(key);
            //}
            //if (Keys.Contains(key)) throw new Exception();

            sourceSegment[index] = new KeyValuePair<string, T>(key, value);

            Parent.ON_ADD(index, key, value);
            ON_ADD(index, key, value);
        }
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => sourceSegment.Where(kvp => kvp != null).Select(kvp => kvp!.Value.Value).GetEnumerator();

    }
}
