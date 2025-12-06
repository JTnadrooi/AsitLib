using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib
{
    /// <summary>
    /// Represents a dictionary that provides a default value when a key is not found.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary. This type must not be <see langword="null"/>.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    public class DefaultDictionary<TKey, TValue> : IDictionary<TKey, TValue> where TKey : notnull
    {
        private readonly Dictionary<TKey, TValue> _inner;
        private readonly Func<TKey, TValue> _defaultValueFactory;

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// If the key does not exist, the default value is returned.
        /// </summary>
        /// <param name="key">The key whose value to get or set.</param>
        /// <returns>The value associated with the specified key, or the generated default value if the key is not found.</returns>
        public TValue this[TKey key]
        {
            get
            {
                if (!_inner.TryGetValue(key, out var value)) value = _defaultValueFactory.Invoke(key);
                return value;
            }
            set => _inner[key] = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultDictionary{TKey, TValue}"/> class with a fixed default value.
        /// </summary>
        /// <param name="defaultValue">The default value to return when a key is not found in the dictionary.</param>
        /// <param name="capacity">The optional initial capacity of the dictionary.</param>
        public DefaultDictionary(TValue defaultValue, int? capacity = null) : this(k => defaultValue, capacity) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultDictionary{TKey, TValue}"/> class with a default value factory.
        /// </summary>
        /// <param name="defaultValueFactory">A function that generates the default value when a key is not found in the dictionary.</param>
        /// <param name="capacity">The optional initial capacity of the dictionary.</param>
        public DefaultDictionary(Func<TValue> defaultValueFactory, int? capacity = null) : this(k => defaultValueFactory.Invoke(), capacity) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultDictionary{TKey, TValue}"/> class with a custom default value factory.
        /// </summary>
        /// <param name="defaultValueFactory">A function that generates the default value based on the key when a key is not found in the dictionary.</param>
        /// <param name="capacity">The optional initial capacity of the dictionary.</param>
        public DefaultDictionary(Func<TKey, TValue> defaultValueFactory, int? capacity = null)
        {
            _inner = capacity.HasValue ? new Dictionary<TKey, TValue>(capacity.Value) : new Dictionary<TKey, TValue>();
            _defaultValueFactory = defaultValueFactory;
        }

        /// <summary>
        /// Attempts to get the value associated with the specified key. 
        /// This method always returns <see langword="true"/> since the dictionary will always generate or retrieve a value for the given key, using the default value factory if necessary.
        /// </summary>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">The value associated with the specified key. If the key does not exist, the default value will be generated.</param>
        /// <returns>
        /// Always returns <see langword="true"/> since the dictionary guarantees the key will exist after this method 
        /// (either with an existing value or a generated default value).
        /// </returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            value = this[key];
            return true;
        }

        public ICollection<TKey> Keys => _inner.Keys;
        public ICollection<TValue> Values => _inner.Values;
        public int Count => _inner.Count;
        public bool IsReadOnly => false;
        public void Add(TKey key, TValue value) => _inner.Add(key, value);
        public bool ContainsKey(TKey key) => _inner.ContainsKey(key);
        public bool Remove(TKey key) => _inner.Remove(key);
        public void Add(KeyValuePair<TKey, TValue> item) => _inner.Add(item.Key, item.Value);
        public void Clear() => _inner.Clear();
        public bool Contains(KeyValuePair<TKey, TValue> item) => _inner.Contains(item);
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => ((ICollection<KeyValuePair<TKey, TValue>>)_inner).CopyTo(array, arrayIndex);
        public bool Remove(KeyValuePair<TKey, TValue> item) => _inner.Remove(item.Key);
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _inner.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _inner.GetEnumerator();
    }
}
