using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib
{
    public static class TupleExtensions
    {
        public static object?[] GetItems(this ITuple tuple)
        {
            object?[] toret = new object?[tuple.Length];
            for (int i = 0; i < tuple.Length; i++) toret[i] = tuple[i];
            return toret;
        }
    }
}
