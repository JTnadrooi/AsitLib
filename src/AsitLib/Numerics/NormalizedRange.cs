using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Text;

namespace AsitLib.Numerics
{
    /// <summary>
    /// A downgraded <see cref="System.Range"/> for simple math operations.
    /// </summary>
    public readonly struct NormalizedRange : IEquatable<NormalizedRange>
    {
        /// <summary>
        /// Gets if the <see cref="Start"/> and <see cref="End"/> are the same.
        /// </summary>
        public bool IsEmpty => Start == End;

        /// <summary>
        /// Gets the inclusive start of this <see cref="NormalizedRange"/>.
        /// </summary>
        public int Start { get; }

        /// <summary>
        /// Gets the exclusive end of this <see cref="NormalizedRange"/>.
        /// </summary>
        public int End { get; }

        /// <summary>
        /// Gets the lenght of the sequence this <see cref="NormalizedRange"/> covers.
        /// </summary>
        public readonly int Lenght { get; }

        /// <summary>
        /// Create a new <see cref="NormalizedRange"/> with set values.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public NormalizedRange(int start, int end)
        {
            if (start > end) throw new InvalidOperationException($"Start is a higher number than End; '{start}' > '{end}'");
            Start = start;
            End = end;
            Lenght = End - Start;
        }

        /// <summary>
        /// Create a new <see cref="NormalizedRange"/> from a existing <see cref="Range"/> given the specified <paramref name="collectionLenght"/>.
        /// </summary>
        /// <param name="source">The <paramref name="source"/> <see cref="Range"/></param>
        /// <param name="collectionLenght">The lenght of the collection the <paramref name="source"/> <see cref="Range"/> covers.</param>
        public NormalizedRange(Range source, int collectionLenght)
        {
            (Start, End) = source.GetStartEnd(collectionLenght);
        }

        /// <summary>
        /// Create a new <see cref="NormalizedRange"/> from a existing <see cref="Range"/>. 
        /// This will throw an <see cref="ArgumentException"/> if the <paramref name="source"/> <see cref="Range"/> is not normalized. See <see cref="RangeExtensions.IsNormalized(Range)"/>.
        /// </summary>
        /// <param name="source">The <paramref name="source"/> <see cref="Range"/></param>
        /// <param name="collectionLenght">The lenght of the collection the <paramref name="source"/> <see cref="Range"/> covers.</param>
        public NormalizedRange(Range source)
        {
            if (source.IsNormalized())
            {
                Start = source.Start.Value;
                End = source.End.Value;
            }
            else throw new ArgumentException(nameof(source), "Range not normalized.");
        }

        public readonly bool Equals([AllowNull] NormalizedRange other)
            => Start == other.Start && End == other.End;

        /// <summary>
        /// Gets a <see cref="bool"/> indicating if the given <see cref="int"/> falls within this <see cref="NormalizedRange"/>.
        /// </summary>
        /// <param name="i">The <see cref="int"/> to use for the calculation.</param>
        /// <returns>A <see cref="bool"/> indicating of the <see cref="int"/> falls within the range of this <see cref="NormalizedRange"/>.</returns>
        public readonly bool Contains(int i) => i >= Start && i < End;

        public void Deconstruct(out int start, out int end)
        {
            start = Start;
            end = End;
        }

        /// <summary>
        /// Gets this <see cref="NormalizedRange"/> as a normal <see cref="Range"/>.
        /// </summary>
        /// <returns>This <see cref="NormalizedRange"/> as a normal <see cref="Range"/>.</returns>
        public Range AsRange() => new Range(Start, End);

        public override string ToString() => AsRange().ToString();

        public static implicit operator Range(NormalizedRange normalizedRange) => normalizedRange.AsRange();

        /// <summary>
        /// Gets a <see cref="NormalizedRange"/> from two values. <paramref name="value1"/> does not have to be the <see cref="NormalizedRange.Start"/> and <paramref name="value2"/> does not have to be the <see cref="NormalizedRange.End"/>.
        /// </summary>
        /// <param name="value1">The first value.</param>
        /// <param name="value2">The second value.</param>
        /// <returns>A <see cref="NormalizedRange"/> ranging the difference between the two values.</returns>
        public static NormalizedRange GetFromValues(int value1, int value2)
            => new NormalizedRange(System.Math.Min(value1, value2), System.Math.Max(value1, value2));

        public static NormalizedRange CreateFromFactory<T>(Range source, IEnumerable<T> values)
        {
            if (source.IsNormalized()) return new NormalizedRange(source);
            else return new NormalizedRange(source, values.Count());
        }
    }

    /// <summary>
    /// Extension methods for working with <see cref="Range"/> instances.
    /// </summary>
    public static class RangeExtensions
    {
        /// <summary>
        /// Converts a <see cref="Range"/> to a <see cref="NormalizedRange"/> based on the collection's length.
        /// </summary>
        /// <param name="range">The <see cref="Range"/> to normalize.</param>
        /// <param name="collectionLength">The length of the collection.</param>
        /// <returns>A <see cref="NormalizedRange"/> representing the input <paramref name="range"/>, normalized.</returns>
        public static NormalizedRange ToNormalizedRange(this Range range, int collectionLength)
            => new NormalizedRange(range, collectionLength);

        /// <summary>
        /// Converts a <see cref="Range"/> to a <see cref="NormalizedRange"/>.
        /// </summary>
        /// <param name="range">The <see cref="Range"/> to normalize.</param>
        /// <returns>A <see cref="NormalizedRange"/> representing the input <paramref name="range"/>, normalized.</returns>
        public static NormalizedRange ToNormalizedRange(this Range range)
            => new NormalizedRange(range);

        /// <summary>
        /// Gets if the <paramref name="range"/> is normalized.
        /// </summary>
        /// <param name="range"></param>
        /// <returns><see langword="true"/> if both <see cref="Range.Start"/> and <see cref="Range.End"/> do not make use of <see cref="Index.IsFromEnd"/>; otherwise, <see langword="false"/>.</returns>
        public static bool IsNormalized(this Range range)
            => !(range.Start.IsFromEnd || range.End.IsFromEnd);

        /// <summary>
        /// Attempts to get the length of the <paramref name="range"/>.
        /// </summary>
        /// <param name="range">The <see cref="Range"/> to calculate the length for.</param>
        /// <param name="length">The length of the <see cref="Range"/> if successful; otherwise, <see langword="default"/>.</param>
        /// <returns><see langword="true"/> if the length was successfully calculated, <see langword="false"/> if the <paramref name="range"/> has <see cref="Index.IsFromEnd"/> values.</returns>
        public static bool TryGetLength(this Range range, out int length)
        {
            if (!range.IsNormalized())
            {
                length = default;
                return false;
            }
            length = range.End.Value - range.Start.Value;
            return true;
        }

        /// <summary>
        /// Attempts to determine whether a specific value is within the <paramref name="range"/>.
        /// </summary>
        /// <param name="range">The <see cref="Range"/> to check.</param>
        /// <param name="value">The value to check for containment.</param>
        /// <param name="contains"><see langword="true"/> if the value is contained within the range; otherwise, <see langword="false"/>.</param>
        /// <returns><see langword="true"/> if the value is successfully checked against the range, <see langword="false"/> if the range has "FromEnd" values.</returns>
        public static bool TryContains(this Range range, int value, out bool contains)
        {
            if (!range.IsNormalized())
            {
                contains = false;
                return false;
            }
            contains = range.Start.Value <= value && value < range.End.Value;
            return true;
        }

        /// <summary>
        /// Attempts to convert the <paramref name="range"/> to a <see cref="NormalizedRange"/>.
        /// </summary>
        /// <param name="range">The <see cref="Range"/> to convert.</param>
        /// <param name="normalizedRange">The normalized range if successful; otherwise, <see langword="default"/>.</param>
        /// <returns><see langword="true"/> if the range was successfully normalized, <see langword="false"/> if the range has <see cref="Index.IsFromEnd"/> values.</returns>
        public static bool TryToNormalizedRange(this Range range, out NormalizedRange normalizedRange)
        {
            if (!range.IsNormalized())
            {
                normalizedRange = default;
                return false;
            }
            normalizedRange = new NormalizedRange(range.Start.Value, range.End.Value);
            return true;
        }

        /// <summary>
        /// Returns the start and end indices of the <paramref name="range"/> based on the <paramref name="collectionLength"/>.
        /// </summary>
        /// <param name="range">The <see cref="Range"/> to get the start and end indices for.</param>
        /// <param name="collectionLength">The length of the collection covered by the <paramref name="range"/>.</param>
        /// <returns>A tuple containing the start and end indices of the <paramref name="range"/>.</returns>
        public static (int Start, int End) GetStartEnd(this Range range, int collectionLength)
        {
            var (Offset, Length) = range.GetOffsetAndLength(collectionLength);
            return (Offset, Offset + Length);
        }

        /// <summary>
        /// Checks whether a specific value is contained within the <paramref name="range"/>, considering the <paramref name="collectionLength"/>.
        /// </summary>
        /// <param name="range">The <see cref="Range"/> to check.</param>
        /// <param name="i">The index to check for.</param>
        /// <param name="collectionLength">The length of the collection.</param>
        /// <returns><see langword="true"/> if the index is contained within the <paramref name="range"/>; otherwise, <see langword="false"/>.</returns>
        public static bool Contains(this Range range, int i, int collectionLength)
            => range.ToNormalizedRange(collectionLength).Contains(i);
    }
}
