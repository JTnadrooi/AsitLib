using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace AsitLib
{
    public static class SRangeExtensions
    {
        public static AsitRange ToAsitRange(this Range range, int collectionLenght) 
            => new AsitRange(range, collectionLenght);
        public static void TryGetLenght(this Range range, out int lenght) 
            => lenght = range.Start.IsFromEnd || range.End.IsFromEnd ? default : range.End.Value - range.Start.Value;
        public static void TryContains(this Range range, int value, out bool contains) 
            => contains = range.Start.IsFromEnd || range.End.IsFromEnd ? default : range.Start.Value >= value && value < range.End.Value;
        public static void TryToAsitRange(this Range range, out AsitRange asitRange) 
            => asitRange = range.Start.IsFromEnd || range.End.IsFromEnd ? default : new AsitRange(range.Start.Value, range.End.Value);
        public static (int Start, int End) GetStartEnd(this Range range, int collectionLenght)
        {
            var (Offset, Length) = range.GetOffsetAndLength(collectionLenght);
            return (Offset, Offset + Length);
        }
        public static bool Contains(this Range range, int i, int collectionLenght)
            => range.ToAsitRange(collectionLenght).Contains(i);
    }
}
