using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib
{
    public sealed class TypeDictionary<TParentType> : IDictionary<Type, TParentType>,
        ICollection<KeyValuePair<Type, TParentType>>,
        IEnumerable<KeyValuePair<Type, TParentType>>,
        IReadOnlyCollection<KeyValuePair<Type, TParentType>>,
        IReadOnlyDictionary<Type, TParentType>
    {
        private readonly Dictionary<Type, TParentType> _dictionary;

        public TParentType this[Type key]
        {
            get => _dictionary[key];
            set => _dictionary[key] = value;
        }

        public TypeDictionary(int capacity)
        {
            _dictionary = new Dictionary<Type, TParentType>(capacity);
        }

        public TypeDictionary()
        {
            _dictionary = new Dictionary<Type, TParentType>();
        }

        public ICollection<Type> Keys => _dictionary.Keys;
        public ICollection<TParentType> Values => _dictionary.Values;
        public int Count => _dictionary.Count;
        public bool IsReadOnly => false;

        IEnumerable<Type> IReadOnlyDictionary<Type, TParentType>.Keys => ((IReadOnlyDictionary<Type, TParentType>)_dictionary).Keys;

        IEnumerable<TParentType> IReadOnlyDictionary<Type, TParentType>.Values => ((IReadOnlyDictionary<Type, TParentType>)_dictionary).Values;

        private void ThrowIfInvalidKey(Type key, TParentType value)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(value);

            if (!key.IsAssignableTo(typeof(TParentType))) throw new ArgumentException($"{key} is not assignable to {typeof(TParentType)}", nameof(key));
        }

        public void Add<T>(T value) where T : TParentType => Add(typeof(T), value);
        public void Add(KeyValuePair<Type, TParentType> item) => Add(item.Key, item.Value);
        public void Add(Type key, TParentType value)
        {
            ThrowIfInvalidKey(key, value);

            _dictionary.Add(key, value);
        }

        public T GetValue<T>() where T : TParentType => (T)this[typeof(T)]!;

        public void Clear() => _dictionary.Clear();

        public bool Contains(KeyValuePair<Type, TParentType> item) => _dictionary.Contains(item);
        public bool ContainsType<T>() where T : TParentType => ContainsKey(typeof(T));
        public bool ContainsKey(Type key) => _dictionary.ContainsKey(key);

        public void CopyTo(KeyValuePair<Type, TParentType>[] array, int arrayIndex) =>
            ((ICollection<KeyValuePair<Type, TParentType>>)_dictionary).CopyTo(array, arrayIndex);

        public IEnumerator<KeyValuePair<Type, TParentType>> GetEnumerator() => _dictionary.GetEnumerator();

        public void Remove<T>() where T : TParentType => Remove(typeof(T));
        public bool Remove(Type key) => _dictionary.Remove(key);
        public bool Remove(KeyValuePair<Type, TParentType> item) => _dictionary.Remove(item.Key);

        public bool TryGetValue<T>([MaybeNullWhen(false)] out TParentType value) => TryGetValue(typeof(T), out value);

        public bool TryGetValue(Type key, [MaybeNullWhen(false)] out TParentType value) => _dictionary.TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator() => _dictionary.GetEnumerator();
    }
}