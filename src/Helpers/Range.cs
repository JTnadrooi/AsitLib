using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Numerics;

namespace AsitLib
{
    /// <summary>
    /// A downgraded <see cref="System.Range"/> more usefull for simple math operations.
    /// </summary>
    public readonly struct AsitRange : IEquatable<AsitRange>, ICloneable
    {
        public bool IsEmpty => Start == End;
        /// <summary>
        /// The start of this <see cref="Range"/>. <strong>Inclucive!</strong>
        /// </summary>
        public int Start { get; }
        /// <summary>
        /// The end of this <see cref="Range"/>. <strong>Exclucive!</strong>
        /// </summary>
        public int End { get; }
        /// <summary>
        /// The lenght of the sequence this <see cref="AsitRange"/> covers.
        /// </summary>
        public readonly int Lenght => End - Start;
        /// <summary>
        /// The underlying <see cref="Range"/>.
        /// </summary>
        public readonly Range Range => new Range(Start, End);
        /// <summary>
        /// Create a new <see cref="AsitRange"/> with set values.
        /// </summary>
        /// <param name="value1">The start of the <see cref="Range"/>. <strong>Inclucive!</strong></param>
        /// <param name="value2">The end of this <see cref="Range"/>. <strong>Exclucive!</strong></param>
        /// <param name="throwEx">
        /// If <see langword="true"/>, a <see cref="InvalidOperationException"/> will be throw when value1 > value2. 
        /// If <see langword="false"/>, the <see cref="Start"/> and <see cref="End"/> will be set to the smaller and larger automatically,
        /// </param>
        public AsitRange(int value1, int value2, bool throwEx = true)
        {
            if (throwEx)
            {
                if(value1 > value2) throw new InvalidOperationException();
                Start = value1;
                End = value2;
            }
            else
            {
                if(value1 == value2)
                {
                    Start = value1;
                    End = value2;
                }
                else
                {
                    Start = System.Math.Min(value1, value2);
                    End = System.Math.Max(value1, value2);
                }
            }
            //Console.WriteLine(start + " " + end);
            //Indexes = new int[start - (end - 1) + 1]
        }
        /// <summary>
        /// Create a new <see cref="AsitRange"/> from a existing <see cref="Range"/> given the size of the collection. (<paramref name="collectionLenght"/>)
        /// </summary>
        /// <param name="source">The <paramref name="source"/> <see cref="Range"/></param>
        /// <param name="collectionLenght">The lenght of the collection the <paramref name="source"/> <see cref="Range"/> covers.</param>
        public AsitRange(Range source, int collectionLenght)
        {
            var a = source.GetStartEnd(collectionLenght);
            Start = a.Start;
            End = a.End;
        }
        public readonly bool Equals([AllowNull] AsitRange other)
            => Start == other.Start && End == other.End;
        public readonly object Clone() => new AsitRange(Start, End);
        /// <summary>
        /// Get a <see cref="bool"/> indicating if the given <see cref="int"/> falls within the range of this <see cref="AsitRange"/>.
        /// </summary>
        /// <param name="l">The <see cref="int"/> to use for the calculation.</param>
        /// <returns>A <see cref="bool"/> indicating of the <see cref="long"/> falls within the range of this <see cref="AsitRange"/>.</returns>
        public readonly bool Contains(int l) => l >= Start && l < End;
        /// <summary>
        /// Get a <see cref="bool"/> indicating if the given <see cref="int"/> falls within the range of this <see cref="AsitRange"/>.
        /// </summary>
        /// <param name="l">The <see cref="long"/> to use for the calculation.</param>
        /// <returns>A <see cref="bool"/> indicating of the <see cref="long"/> falls within the range of this <see cref="AsitRange"/>.</returns>
        public readonly bool Contains(long l) => l >= Start && l < End;
        public override string ToString() => Range.ToString();
        public static implicit operator Range(AsitRange asitRange) => asitRange.Range;
    }
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
