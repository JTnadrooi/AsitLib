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
    /// <summary>
    /// Provides the base <see langword="interface"/> for the abstraction of non generic AsitDictionaries.
    /// </summary>
    public interface ISegmentedMap
    {
        /// <summary>
        /// The max <see cref="Capacity"/> of this <see cref="ISegmentedMap"/>.
        /// </summary>
        public int Capacity { get; }
        /// <summary>
        /// The remaining space of this <see cref="ISegmentedMap"/>.
        /// </summary>
        public int Space { get; }
        /// <summary>
        /// A value indicating of this <see cref="ISegmentedMap"/> has place for at least one more item.
        /// </summary>
        public bool HasSpace { get; }
        /// <summary>
        /// Get the index of the given <see cref="string"/> <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The <see cref="KeyValuePair{TKey, TValue}.Key"/> to search.</param>
        /// <returns>The index of the <see cref="KeyValuePair{TKey, TValue}"/> where the <see cref="KeyValuePair{TKey, TValue}.Key"/> matches the given <paramref name="key"/>.</returns>
        public int GetIndex(string key);
        /// <summary>
        /// Get the <see cref="KeyValuePair{TKey, TValue}.Key"/> value of the <see cref="KeyValuePair{TKey, TValue}"/> at the given <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The index to get the <see cref="KeyValuePair{TKey, TValue}.Key"/> from.</param>
        /// <returns></returns>
        public string GetKey(int index);
    }
    /// <summary>
    /// Provides the base <see langword="interface"/> for the abstraction of generic AsitDictionaries.
    /// </summary>
    public interface ISegmentedMap<T> : IDictionary<string, T>, ISegmentedMap, IEnumerable<T>
    {
        /// <summary>
        /// Get a shallow array copy of all the values from the <see cref="Source"/> array. <see langword="null"/>/empty values excluded.
        /// </summary>
        /// <returns>A shallow array copy of all the values from the <see cref="Source"/> array. <see langword="null"/>/empty values excluded.</returns>
        public T[] GetAll();
        /// <summary>
        /// Add a new <see cref="KeyValuePair"/> at the given <paramref name="index"/>. 
        /// If the <see cref="KeyValuePair{TKey, TValue}"/> at that <paramref name="index"/> isn't <see langword="null"/>, a <see cref="Exception"/> is thrown.
        /// </summary>
        /// <param name="index">The index where the <see cref="KeyValuePair{TKey, TValue}"/> will be placed. (If free.)</param>
        /// <param name="key">The <see cref="KeyValuePair{TKey, TValue}.Key"/>.</param>
        /// <param name="value">The <see cref="KeyValuePair{TKey, TValue}.Value"/>.</param>
        public void Add(int index, string key, T value);
        /// <summary>
        /// Add a new <see cref="KeyValuePair"/> at the given <paramref name="index"/>. 
        /// If the <see cref="KeyValuePair{TKey, TValue}"/> at that <paramref name="index"/> isn't <see langword="null"/>, a <see cref="Exception"/> is thrown.
        /// </summary>
        /// <param name="index">
        /// The index where the <see cref="KeyValuePair{TKey, TValue}"/> will be placed and
        /// the <see cref="KeyValuePair{TKey, TValue}"/> of the generated <see cref="KeyValuePair{TKey, TValue}"/>. (If the keys is avalible and if the index is free.)
        /// </param>
        /// <param name="value">The <see cref="KeyValuePair{TKey, TValue}.Value"/>.</param>
        public void Add(int index, T value);
        /// <summary>
        /// Execute a <see cref="Action{T}"/> for every (not <see langword="null"/>) item in the <see cref="Source"/>.
        /// </summary>
        /// <param name="action">The <see cref="Action{T}"/> <see cref="Delegate"/> to execute for every (not <see langword="null"/>) item in the <see cref="Source"/>.</param>
        public void ForEach(Action<T> action);
        /// <summary>
        /// Execute a <see cref="Action{T}"/> for every (not <see langword="null"/>) item in the <see cref="Source"/>.
        /// </summary>
        /// <param name="action">The <see cref="Action{T}"/> <see cref="Delegate"/> to execute for every (not <see langword="null"/>) item in the <see cref="Source"/>.</param>
        public void ForEach(Action<T, int, string> action);
        /// <summary>
        /// Get the <see cref="KeyValuePair{TKey, TValue}.Value"/> of the <see cref="KeyValuePair{TKey, TValue}"/> with the given <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key to search for.</param>
        /// <returns>The <see cref="KeyValuePair{TKey, TValue}.Value"/> of the <see cref="KeyValuePair{TKey, TValue}"/> with the given <paramref name="key"/>.</returns>
        public T Get(string key);
        /// <summary>
        /// Get the <see cref="KeyValuePair{TKey, TValue}.Value"/> of the <see cref="KeyValuePair{TKey, TValue}"/> at the given <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The <paramref name="index"/> to get the <see cref="KeyValuePair{TKey, TValue}.Value"/> from.</param>
        /// <returns>The <see cref="KeyValuePair{TKey, TValue}.Value"/> of the <see cref="KeyValuePair{TKey, TValue}"/> at the given <paramref name="index"/>.</returns>
        public T Get(int index);
        /// <summary>
        /// A <see cref="Action{T1, T2}"/> to execute when a item is added to this <see cref="ISegmentedMap{T}"/>.
        /// </summary>
        public Action<KeyValuePair<string, T>, ISegmentedMap<T>>? OnAdd { get; }
        public Action<KeyValuePair<string, T>, ISegmentedMap<T>>? OnRemove { get; }
        public Action<ISegmentedMap<T>>? OnClear { get; }
        /// <summary>
        /// The <see cref="Source"/> of this <see cref="ISegmentedMap{T}"/> in form of a <see cref="ReadOnlyCollection{T}"/>.
        /// </summary>
        public ReadOnlyCollection<KeyValuePair<string, T>?> Source { get; }
        /// <summary>
        /// Get the value at the given <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The index to get the value from. See also <see cref="SegmentedMap{T}.Get(int)"/>.</param>
        /// <returns>The value at the given <paramref name="index"/>.</returns>
        public T this[int index] { get; set; }
    }
    /// <summary>
    /// Provides a way to manage a set dictiory with advanced add- get- methods and segmentation support.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> that gets stored.</typeparam>
    public class SegmentedMap<T> : ISegmentedMap<T>
    {
        public int Capacity { get; }
        public int Space => Capacity - Count;
        public int Count => Keys.Count;
        public bool HasSpace => Space > 0;
        public bool IsReadOnly => false;
        public ReadOnlyCollection<SegmentedMapSegment<T>> Segments => segments.AsReadOnly();
        private readonly List<SegmentedMapSegment<T>> segments;


        internal KeyValuePair<string,T>?[] source;
        public ReadOnlyCollection<KeyValuePair<string, T>?> Source => new ReadOnlyCollection<KeyValuePair<string, T>?>(source);

        public ICollection<string> Keys { get; private set; }
        public ICollection<T> Values => source.WhereSelect(kvp => kvp == null ? (default(T)!, false) : (kvp.Value.Value, true)).ToList();

        public Action<KeyValuePair<string, T>, ISegmentedMap<T>>? OnRemove { get; set; }
        public Action<KeyValuePair<string, T>, ISegmentedMap<T>>? OnAdd { get; set; }
        public Action<ISegmentedMap<T>>? OnClear { get; set; }


        //public ICollection<T> Values {get; private set;}

        public T this[int index]
        {
            get => Get(index);
            set => Add(index, value);
        }

        //public ICollection<string> Keys { get; private set; }
        //public ICollection<T> Values { get; private set; }


        public T this[string key]
        {
            get => Get(key);
            set
            {
                try
                {
                    source[GetIndex(key)] = new KeyValuePair<string, T>(key, value);
                }
                catch
                {
                    Add(key, value);
                }
            }
        }


        public SegmentedMap(T[] source) : this(GeneralHelpers.ToKeyValuePair(source)) { }
        public SegmentedMap(T[] source, int capacity) : this(GeneralHelpers.ToKeyValuePair(source), capacity) { }
        public SegmentedMap(int capacity) : this(new KeyValuePair<string, T>?[capacity]) { }
        public SegmentedMap(KeyValuePair<string, T>?[] source) : this(source, source.Length) { }


        public SegmentedMap(KeyValuePair<string, T>?[] source, int capacity)
        {
            if (capacity < source.Length) throw new Exception();
            this.source = new KeyValuePair<string, T>?[capacity];
            source.CopyTo(this.source, 0);
            Capacity = capacity;
            segments = new List<SegmentedMapSegment<T>>();
            //Values = new Collection<T>(source.WhereSelect<KeyValuePair<string, T>?, T>(kvp => kvp == null ? (default(T)!, false) : (kvp.Value.Value, true)).ToList());
            Keys = new Collection<string>(source.WhereSelect<KeyValuePair<string, T>?, string>(kvp => kvp == null ? (string.Empty, false) : (kvp.Value.Key, true)).ToList());
        }
        //O(n)
        //O(n3)
        public SegmentedMapSegment<T> AddSegment(Index start, Index end) => AddSegment(new Range(start, end));
        public SegmentedMapSegment<T> AddSegment(Range range) => AddSegment(new SegmentedMapSegment<T>(this, range));
        public SegmentedMapSegment<T> AddSegment(SegmentedMapSegment<T> segment)
        {
            segments.Add(segment);
            return segment;
        }
        public T[] GetAll() => Values.ToArray();
        //O(1)
        public T Get(string key) => Get(GetIndex(key));
        //O(1)
        public T Get(int index) => source[index]!.Value.Value;
        public int GetIndex(string id)
        {
            for (int i = 0; i < source.Length; i++)
                if (source[i] != null)
                    if (source[i]!.Value.Key == id)
                        return i;
            throw new Exception();
        }
        public string GetKey(int index)
        {
            if (source[index] == null) throw new Exception();
            return source[index]!.Value.Key;
        }
        public void Add(KeyValuePair<string, T> item) => Add(item.Key, item.Value);
        public void Add(string key, T value)
        {
            if (ContainsKey(key)) throw new Exception();
            for (int i = 0; i < source.Length; i++)
                if (source[i] == null)
                {
                    Add(i, key, value);
                    return;
                }
            throw new Exception();
        }
        public void Add(int index, T value) => Add(index, index.ToString(), value);
        public void Add(int index, string key, T value)
        {
            if (index > Capacity) throw new Exception();
            if (source[index] != null) throw new Exception();
            //if (Keys.Contains(key)) throw new Exception();
            source[index] = new KeyValuePair<string, T>(key, value);
            ON_ADD(index, key, value);
            foreach (SegmentedMapSegment<T> segment in segments)    
            {
                if (segment.NormalizedRange.Contains(index))
                {
                    //Console.WriteLine(segment.AsitRange.ToString());
                    segment.ON_ADD(index, key, value);
                }
            }
            //ON_ADD(index, key, value);
            //source[index] = new KeyValuePair<string, T>(key, value);
        }
        //{
        //    for (int i = 0; i < source.Length; i++)
        //        if (source[i] == null)
        //        {
        //            source[i] = new KeyValuePair<string, T>(key, value);

        //            Count++;
        //        }
        //    throw new Exception();
        //}
        public bool Contains(KeyValuePair<string, T> item) => ContainsKey(item.Key);
        public bool ContainsKey(string key) => source.Any(kvp => kvp != null && kvp.Value.Key == key);
        public void ForEach(Action<T> action)
            => ForEach((value, index, key) => action.Invoke(value));
        //public void ForEach(Action<KeyValuePair<string, T>> action)
        //    => ForEach((value, index, key) => action.Invoke(new KeyValuePair<string, T>(key, value)));
        public void ForEach(Action <T, int, string> action)
        {
            for (int i = 0; i < source.Length; i++)
                if (source[i] == null) continue;
                else action.Invoke(source[i]!.Value.Value, i, source[i]!.Value.Key);
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
        public void Clear()
        {
            Keys.Clear();
            source.Select(kvp => kvp = null);
            foreach (SegmentedMapSegment<T> segment in segments)
                segment.ON_CLEAR();
            OnClear?.Invoke(this);
        }
        public void CopyTo(KeyValuePair<string, T>[] array, int arrayIndex) => source.CopyTo(array, arrayIndex);
        public bool Remove(KeyValuePair<string, T> item) => Remove(item.Key);
        public bool Remove(string key)
        {
            for (int i = 0; i < source.Length; i++)
                if (source[i] == null) return false;
                else if (source[i]!.Value.Key == key)
                    return Remove(i);
            return false;
        }
        public bool Remove(int index)
        {
            if (source[index] == null || source[index]!.Value.Value!.Equals(source[index]!.Value.Value)) return false;
            ON_REMOVE(source[index]!.Value.Key, source[index]!.Value.Value);
            foreach (SegmentedMapSegment<T> segment in segments)
            {
                if (segment.NormalizedRange.Contains(index)) segment.ON_REMOVE(source[index]!.Value.Key, source[index]!.Value.Value);
            }
            source[index] = null;
            return true;
        }
        //O(2n)
        public IEnumerator<KeyValuePair<string, T>> GetEnumerator() => source.Where(kvp => kvp != null).Select(kvp => kvp!.Value).GetEnumerator();
        //O(2n)
        IEnumerator IEnumerable.GetEnumerator() => source.Where(kvp => kvp != null).Select(kvp => kvp!.Value).GetEnumerator();
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => source.Where(kvp => kvp != null).Select(kvp => kvp!.Value.Value).GetEnumerator();
        //internal void AddUpdate(KeyValuePair<string, T> keyValuePair)
        //{
        //    Keys.Add(keyValuePair.Key);
        //    Values.Add(keyValuePair.Value);
        //}
        //private void BG_ADD(string key, T value, int index)
        //{
        //    Count++;
        //    foreach (var segment in Segments)
        //    {
        //        if (AsitLibStatic.ToSimpleRange(segment.Range, Capacity).c)
        //    }
        //}
        internal void ON_ADD(int index, string key, T value)
        {
            //Console.WriteLine("PARENT KNOWS!");
            Keys.Add(key);
            Console.WriteLine("Adding: " + value!.ToString() + " at " + index);
            OnAdd?.Invoke(new KeyValuePair<string, T>(key, value), this);
        }
        internal void ON_CLEAR(SegmentedMapSegment<T> source)
        {
            //Console.WriteLine("C - PARENT KNOWS!");
            foreach (string key in source.Keys)
                Keys.Remove(key);
            //keys and values management.
        }
        internal void ON_REMOVE(string key, T value)
        {
            Keys.Remove(key);
            OnRemove?.Invoke(new KeyValuePair<string, T>(key, value), this);
        }
    }
}
