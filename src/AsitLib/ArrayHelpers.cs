using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib
{
    public static class ArrayHelpers
    {
        public static T[] Combine<T>(T[] array, T item) => Combine(array, [item]);
        public static T[] Combine<T>(T item, T[] array) => Combine([item], array);
        public static T[] Combine<T>(T[] array1, T[] array2) => array1.Concat(array2).ToArray();
    }
}
