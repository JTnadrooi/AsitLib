using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib.Collections
{
    public static class AsitEnumerable
    {
        public static bool EndsWith<T>(this T[] source, T[] value)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (value == null) throw new ArgumentNullException(nameof(value));
            if(value.Length > source.Length) throw new ArgumentException(nameof(value) + " lenght exeeds base array lenght.");

            return source[(source.Length - value.Length)..].SequenceEqual(value);
        }
        public static void SwitchIndexes<T>(ref T[] source, int index1, int index2)
        {
            T item1 = source[index1];
            T item2 = source[index2];

            source[index1] = item2; 
            source[index2] = item1;
        }
        public static void SqueezeIndexes<T>(ref T[] source)
        {
            List<T?> squeezedList = new List<T?>();

            for (int i = 0; i < source.Length; i++)
                if (source[i] != null) squeezedList.Add(source[i]);
            while (squeezedList.Count != source.Length) squeezedList.Add(default(T));

            source = squeezedList.ToArray()!;
        }
        public static void ExtrapolateValues<T>(ref T[] source)
        {
            int nullIndex = source.GetFirstIndexWhere(t => t == null);
            Console.WriteLine(nullIndex);
            while (source.Any(t => t == null))
            {
                AddValuesTo(ref source, source);    
            }
            //Array.Copy(source, 0, source, nullIndex, source.Length - nullIndex);
        }
        public static void SetSize<T>(ref T[] source, int newSize)
        {
            if (source.Length == newSize) return;
            else if(source.Length > newSize)
            {
                source = source[..newSize];
                return;
            }
            else
            {
                List<T> values = new List<T>(source);
                int diff = (int)Math.MathFI.Diff(source.Length, newSize);
                //Console.WriteLine(diff);
                for (int i = 0; i < diff; i++) values.Add(default(T));
                source = values.ToArray();
            }
        }
        public static void AddValuesTo<T>(ref T[] source, params T[] values)
        {
            int startIndex = source.GetFirstIndexWhere(t => t == null);
            for (int i = 0; i < values.Length; i++)
                if (source.Length > i + startIndex && source[startIndex + i] == null) source[startIndex + i] = values[i];
        }
        public static void Shift<T>(ref T[] array, int amount)
        {
            T[] shiftedArray = new T[array.Length];
            int newIndex;

            for (int i = 0; i < array.Length; i++)
            {
                newIndex = i + amount;
                if (newIndex >= 0 && newIndex < array.Length) shiftedArray[newIndex] = array[i];
            }

            array = shiftedArray;
        }
        public static void ShiftIndexes<T>(ref T[] array, int oldIndex, int newIndex)
        {
            if (oldIndex == newIndex) return;
            T atIndex = array[oldIndex];
            if (newIndex < oldIndex) Array.Copy(array, newIndex, array, newIndex + 1, oldIndex - newIndex);
            else Array.Copy(array, oldIndex + 1, array, oldIndex, newIndex - oldIndex);
            array[newIndex] = atIndex;
        }
        public static void PushIndex<T>(ref T[] source, int index)
        {
            ShiftIndexes(ref source, index, 0);
        }
    }
}
