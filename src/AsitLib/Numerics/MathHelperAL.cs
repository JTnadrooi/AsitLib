using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib.Numerics
{
    /// <summary>
    /// Provides helper methods for handling numeric types and operations.
    /// </summary>
    public static class MathHelperAL
    {
        private static string[] _prefixes = { "y", "z", "a", "f", "p", "n", "µ", "m", string.Empty, "k", "M", "G", "T", "P", "E", "Z", "Y" };

        /// <summary>
        /// A collection of numeric types.
        /// Includes <see cref="Half"/>, <see cref="BigInteger"/>, <see cref="Int128"/>, <see cref="UInt128"/> and <see cref="Complex"/>.
        /// See <see cref="IsNumberic(Type)"/>.
        /// </summary>
        public static ImmutableHashSet<Type> NumericTypes { get; } = new HashSet<Type>
        {
            typeof(int), typeof(double), typeof(decimal),
            typeof(long), typeof(short), typeof(sbyte),
            typeof(byte), typeof(ulong), typeof(ushort),
            typeof(uint), typeof(float),
            typeof(Half), typeof(BigInteger), typeof(Int128), typeof(UInt128), typeof(Complex),
        }.ToImmutableHashSet();

        /// <summary>
        /// Determines whether the specified type is a numeric type. See <see cref="NumericTypes"/>.
        /// </summary>
        /// <param name="t">The type to check.</param>
        /// <returns><see langword="true"/> if the specified type is numeric; otherwise, <c>false</c>.</returns>
        public static bool IsNumberic(Type t)
            => NumericTypes.Contains(Nullable.GetUnderlyingType(t) ?? t);

        /// <summary>
        /// Determines whether the specified object is of a numeric type.
        /// </summary>
        /// <param name="value">The object to check.</param>
        /// <returns><see langword="true"/> if the object is of a numeric type; otherwise, <c>false</c>.</returns>
        public static bool IsNumberic(object? value) => value is not null && IsNumberic(value.GetType());

        /// <summary>
        /// Converts the specified numeric value to a <see cref="BigInteger"/>.S
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The <see cref="BigInteger"/> representation of the value.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the value is not numeric.</exception>
        public static BigInteger ToBigInteger(object value)
        {
            if (!IsNumberic(value)) throw new InvalidOperationException();

            if (value is Int128 i1) return i1;
            else if (value is UInt128 i2) return i2;

            return new BigInteger(Convert.ToInt64(value));
        }

        /// <summary>
        /// Determines whether two float values are approximately equal within a given tolerance.
        /// </summary>
        /// <param name="value1">The first value.</param>
        /// <param name="value2">The second value.</param>
        /// <param name="diff">The tolerance value.</param>
        /// <returns><see langword="true"/> if the values are nearly equal; otherwise, <see langword="false"/>.</returns>
        public static bool IsNear(float value1, float value2, float diff)
            => MathAL.Difference(value1, value2) <= diff;

        public static string AddPostfix(int amount)
        {
            int log10 = (int)Math.Log10(Math.Abs(amount));
            if (log10 < -27) return "0.000";
            if (log10 % -3 < 0) log10 -= 3;
            int log1000 = Math.Max(-8, Math.Min(log10 / 3, 8));

            return ((double)amount / Math.Pow(10, log1000 * 3)).ToString("###.###" + _prefixes[log1000 + 8]);
        }
    }
}
