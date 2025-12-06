using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib.Stele
{
    public sealed class SteleMap<TPixel> where TPixel : struct, IEquatable<TPixel>
    {
        public FrozenDictionary<TPixel, int> Map { get; }
        public ReadOnlyCollection<TPixel> InverseMap { get; }
        public int Count => Map.Count;
        public TPixel this[int i] => InverseMap[i];
        public int this[TPixel value] => Map[value];

        private SteleMap(FrozenDictionary<TPixel, int> source)
        {
            Map = source;

            InverseMap = Map.OrderBy(x => x.Value)
                .Select(x => x.Key)
                .ToList().AsReadOnly();
        }

        public override bool Equals(object? obj) => (obj is SteleMap<TPixel> other) ? Map.SequenceEqual(other.Map) : false;
        public override int GetHashCode() => Map.GetHashCode();
        public override string ToString() => $"[{InverseMap.ToJoinedString(", ")}]";

        public static SteleMap<TPixel> Create(IEnumerable<TPixel> values)
        {
            HashSet<TPixel> uniqueValues = new HashSet<TPixel>();

            foreach (TPixel value in values)
            {
                uniqueValues.Add(value);
                if (uniqueValues.Count == 3) break;
            }

            if (uniqueValues.Count < 2) throw new ArgumentException("Values does not contain enough different values.", nameof(values));

            return CreateFromUnique(uniqueValues);
        }

        public static SteleMap<TPixel> CreateFromUnique(TPixel value1, TPixel value2, TPixel value3) => CreateFromUnique([value1, value2, value3]);
        public static SteleMap<TPixel> CreateFromUnique(TPixel value1, TPixel value2) => CreateFromUnique([value1, value2]);
        public static SteleMap<TPixel> CreateFromUnique(IEnumerable<TPixel> values)
        {
            switch (values.Count())
            {
                case 2 or 3:
                    if (values.HasDuplicates()) throw new ArgumentException("Values contain duplicates.", nameof(values));
                    return new SteleMap<TPixel>(values.Select((v, i) => new KeyValuePair<TPixel, int>(v, i)).ToFrozenDictionary());
                default:
                    throw new ArgumentException("Invalid values enumerable size.", nameof(values));
            }
        }
    }
}
