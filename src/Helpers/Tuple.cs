using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace AsitLib.Tuple
{
    public struct AsitValueTuple : ITuple
    {
        public object?[] Values { get; }
        public int Length => Values.Length;
        public object? this[int index] => Values[index];
    }
    public static class AsitTupleExtensions
    {
        public static object?[] GetItems(this ITuple tuple)
        {
            List<object?> toret2 = new List<object?>();
            for (int i = 0; i < tuple.Length; i++)
                toret2.Add(tuple[i]);
            return toret2.ToArray();
        }
    }
}
