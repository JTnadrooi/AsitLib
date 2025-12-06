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

            return CreateFromUnique(uniqueValues);
        }

        public static SteleMap<TPixel> CreateFromUnique(IEnumerable<TPixel> values)
        {
            int count = values.Count();
            if (count > 3 || count < 2) throw new ArgumentException("Invalid source array size.", nameof(values));
            return new SteleMap<TPixel>(values.Select((v, i) => new KeyValuePair<TPixel, int>(v, i)).ToFrozenDictionary());
        }
    }
}
