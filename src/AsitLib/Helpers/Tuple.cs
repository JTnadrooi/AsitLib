using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace AsitLib.Tuple
{
    public struct DynamicTuple : ITuple
    {
        public object?[] Values { get; }
        public int Length => Values.Length;
        public object? this[int index] => Values[index];
    }
}
