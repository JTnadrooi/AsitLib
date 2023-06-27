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
        /// <param name="start">The start of the <see cref="Range"/>. <strong>Inclucive!</strong></param>
        /// <param name="end">The end of this <see cref="Range"/>. <strong>Exclucive!</strong></param>
        public AsitRange(int start, int end)
        {
            Start = start; 
            End = end;
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
}
