using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Numerics;
using static System.Net.Mime.MediaTypeNames;
#nullable enable

namespace AsitLib.Numerics
{
    public static class MathHelperFI
    {
        public static readonly HashSet<Type> NumericTypes = new HashSet<Type>
        {
            typeof(int),  typeof(double),  typeof(decimal),
            typeof(long), typeof(short),   typeof(sbyte),
            typeof(byte), typeof(ulong),   typeof(ushort),
            typeof(uint), typeof(float)
        };
        public static bool IsNumberic(Type t)
            => t == null ? throw new ArgumentNullException() : NumericTypes.Contains(Nullable.GetUnderlyingType(t) ?? t);
        public static bool IsNumberic(object value) => IsNumberic(value.GetType());
        public static BigInteger ToBigInteger(object value)
        {
            if(value == null) throw new ArgumentNullException();
            if (!IsNumberic(value)) throw new InvalidOperationException();
            Type t = Nullable.GetUnderlyingType(value.GetType()) ?? value.GetType();
            return new BigInteger(Convert.ToInt64(value));
        }
        public static NormalizedRotation GetRotation(float rotation, bool isRadiants) => new NormalizedRotation(rotation, isRadiants);
        public static bool IsNear(float value1, float value2, float diff) => System.Math.Abs(value1 - value2) <= diff;
        public static string AddPrefix(int amount, string suffix)
        {
            string[] prefixes = { "nano", "micro", "milli", "kilo", "mega", "giga", "tera" };
            string result = "";
            int prefixIndex = 0;

            // Check if the input value is negative and adjust it to a positive value
            bool isNegative = amount < 0;
            amount = System.Math.Abs(amount);

            while (amount >= 1000 && prefixIndex < prefixes.Length - 1)
            {
                amount /= 1000;
                prefixIndex++;
            }

            string formattedValue = amount.ToString("F2"); // Format to 2 decimal places

            result = $"{formattedValue} {prefixes[prefixIndex]}{suffix}";

            if (isNegative)
            {
                result = "-" + result;
            }

            return result;
        }
    }
    /// <summary>
    /// Provides static methods and constants for extra mathimatical functions.
    /// </summary>
    public static class MathFI
    {
        /// <summary>
        /// The ratio Degrees to Radiants. <br/>  
        /// 2 * <see cref="System.Math.PI"/> / 360 = <see cref="DR"/>
        /// </summary>
        public const float DR = 0.0174533f;
        /// <summary>
        /// Infinite Absolute.<br/>
        /// Returns <see cref="float.NegativeInfinity"/> if <paramref name="value"/> is less than <see langword="0f"/>, <see langword="0f"/> if <paramref name="value"/> is 
        /// equal to <see langword="0f"/> and <see cref="float.PositiveInfinity"/> if <paramref name="value"/> is greater than <see langword="0f"/>.
        /// </summary>
        /// <param name="value">The <see cref="float"/> input value.</param>
        /// <returns>
        /// Returns <see cref="float.NegativeInfinity"/> if <paramref name="value"/> is less than <see langword="0f"/>, <see langword="0f"/> if <paramref name="value"/> is 
        /// equal to <see langword="0f"/> and <see cref="float.PositiveInfinity"/> if <paramref name="value"/> is greater than <see langword="0f"/>.</returns>
        public static float InAbs(float value)
        {
            if (value == float.NegativeInfinity || value == float.PositiveInfinity || float.IsNaN(value)) return value;
            if (value == 0f) return value;
            return value > 0f ? float.PositiveInfinity : float.NegativeInfinity;
        }
        /// <summary>
        /// Advanced Absolute.<br/>
        /// Returns <see langword="-1f"/> if <paramref name="value"/> is less than <see langword="0f"/>, <see langword="0f"/> if <paramref name="value"/> is 
        /// equal to <see langword="0f"/> and <see langword="1f"/> if <paramref name="value"/> is greater than <see langword="0f"/>.
        /// </summary>
        /// <param name="value">The <see cref="float"/> input value.</param>
        /// <returns>
        /// <see langword="-1f"/> if <paramref name="value"/> is less than <see langword="0f"/>, <see langword="0f"/> if <paramref name="value"/> is 
        /// equal to <see langword="0f"/> and <see langword="1f"/> if <paramref name="value"/> is greater than <see langword="0f"/>.</returns>
        public static float AdAbs(float value)
        {
            if (value == float.NegativeInfinity || value == float.PositiveInfinity || float.IsNaN(value)) return value;
            if (value == 0f) return value;
            return value > 0f ? 1f : -1f;
        }
        /// <summary>
        /// Max Absolute.<br/>
        /// Returns <see cref="float.MinValue"/> if <paramref name="value"/> is less than <see langword="0f"/>, <see langword="0f"/> if <paramref name="value"/> is 
        /// equal to <see langword="0f"/> and <see cref="float.MaxValue"/> if <paramref name="value"/> is greater than <see langword="0f"/>.
        /// </summary>
        /// <param name="value">The <see cref="float"/> input value.</param>
        /// <returns>
        /// Returns <see cref="float.MinValue"/> if <paramref name="value"/> is less than <see langword="0f"/>, <see langword="0f"/> if <paramref name="value"/> is 
        /// equal to <see langword="0f"/> and <see cref="float.MaxValue"/> if <paramref name="value"/> is greater than <see langword="0f"/>.</returns>
        public static float MaxAbs(float value)
        {
            if (value == float.NegativeInfinity || value == float.PositiveInfinity || float.IsNaN(value)) return value;
            if (value == 0f) return value;
            return value > 0f ? 1f : -1f;
        }
        /// <summary>
        /// <see cref="Nodge(float, float, float)"/> a <see cref="float"/> <paramref name="value"/> to the desired <paramref name="target"/> with set <paramref name="amount"/>.
        /// </summary>
        /// <param name="value">The <see cref="float"/> input value.</param>
        /// <param name="target">The target for this function to <see cref="Nodge(float, float, float)"/> to.</param>
        /// <param name="amount">The amount the <paramref name="value"/> gets incremented or decremented to reach set <paramref name="target"/>.</param>
        /// <returns>The <paramref name="value"/> nodged to the <paramref name="target"/> with the desired <paramref name="amount"/>.</returns>
        public static float Nodge(float value, float target, float amount)
        {
            if (value < target) return System.Math.Min(value + amount, target);
            else if (value > target) return System.Math.Max(value - amount, target);
            else return target;
        }
        /// <summary>
        /// Returns <see langword="true"/> if the input <paramref name="value"/> is positive, <paramref name="caseZero"/> if the <paramref name="value"/> is equal to <see langword="0f"/>
        /// and <see langword="false"/> if the input in negative.
        /// </summary>
        /// <param name="value">The <see cref="float"/> input value.</param>
        /// <param name="caseZero">What to return if <paramref name="value"/> is equal to <see langword="0f"/>.</param>
        /// <returns><see langword="true"/> if the input <paramref name="value"/> is positive, <paramref name="caseZero"/> if the <paramref name="value"/> is equal to <see langword="0f"/>
        /// and <see langword="false"/> if the input in negative.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static bool ToBoolPosNeg(float value, bool caseZero = false)
        {
            if (float.IsNaN(value)) throw new InvalidOperationException("Value is not a number.");
            if(value == float.NegativeInfinity) return false;
            if (value == float.PositiveInfinity) return true;
            if (value == 0) return caseZero;
            return value > 0f ? true : false;
        }
        /// <summary>
        /// Returns <see langword="true"/> if the <paramref name="value"/> isn't equal to <see langword="0f"/>.
        /// </summary>
        /// <param name="value">The <see cref="float"/> input value.</param>
        /// <returns><see langword="true"/> if the <paramref name="value"/> isn't equal to <see langword="0f"/>.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static bool ToBoolZero(float value)
        {
            if (float.IsNaN(value)) throw new InvalidOperationException("Value is not a number.");
            if (value == 0) return false;
            else return true;
        }
        /// <summary>
        /// Invert a number using the set <paramref name="pointZero"/>. <br/>
        /// <i>Example: Inv(4f, 0f) returns -4f, Inv(4f, 2f) returns 0f, Inv(10f, 8f) returns 6f etc..</i>
        /// </summary>
        /// <param name="value">The <see cref="float"/> input value.</param>
        /// <param name="pointZero">The point zero of the operation.</param>
        /// <returns>The input <paramref name="value"/> inverted using the specified <paramref name="pointZero"/>.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static float Inv(float value, float pointZero = 0f)
        {
            if (value == float.NegativeInfinity || value == float.PositiveInfinity || float.IsNaN(value))
                throw new InvalidOperationException("Value is not a normal number.");
            if (value == pointZero) return value;
            float dif = MathF.Abs(pointZero - value);
            //if (value > pointZero)
            //    return value - dif * 2;
            //return value + dif * 2;
            return value + (dif * 2 * AdAbs(pointZero - value));
        }
        public static float PowZero(float value)
        {
            if (float.IsNaN(value)) throw new InvalidOperationException("Value is not a number.");
            else return 1f;
        }
        public static float PowOne(float value)
        {
            if (float.IsNaN(value)) throw new InvalidOperationException("Value is not a number.");
            else return value;
        }
        public static float Average(params float[] values)
        {
            float toret = values.Sum();
            if (toret == float.NegativeInfinity || toret == float.PositiveInfinity || float.IsNaN(toret))
                throw new InvalidOperationException("Value is not a normal number; average failed");
            return toret / values.Length;
        }
        public static float Fact(float value) => Fact(value, 1);
        public static float Fact(float value, int multiple) => Fact(value, multiple, -1);
        /// <summary>
        /// Advanced factorial most often use for probability calculations.
        /// </summary>
        /// <param name="value">The value to perform this math function on. Negative numbers are allowed, this isn't mathmatically accurate but just letting you know this function isn't stopping you.</param>
        /// <param name="multiple">The amount of exclamationmarks. Negative will throw a <see cref="ArgumentException"/>.</param>
        /// <param name="depth">For how deep this factorial must go. Negative for infinite, zero will just return the input <paramref name="value"/>.</param>
        /// <returns>The input <paramref name="value"/> after being passed through this advanced factorial.</returns>
        public static float Fact(float value, int multiple, int depth) => Fact(value, multiple, depth, x1 => true);
        public static float Fact(float value, Func<float, float, bool> predicate) => Fact(value, 1, -1, x1 => true);
        public static float Fact(float value, int multiple, int depth, Func<float, bool> predicate)
        {
            if (depth == 0) return value;
            if (multiple <= 0) throw new ArgumentException();
            bool negative = value < 0 && AbsDivide(depth < 0 ? System.Math.Abs(value) : depth, multiple) % 2 == 1;
            value = System.Math.Abs(value);
            for (float f = value - 1; f > 0; f--)
                if (predicate.Invoke(f) && (value - f) % multiple == 0 && (value - (1 + f) < depth || depth < 0))
                    value *= f;
            return negative ? -value : value;
        }
        public static int AbsDivide(float value, float divider) => (int)(value / divider);
        /// <summary>
        /// no one cares about gradiants so i can just float values here yay (i could use a enum but again, no one cares about gradiants)
        /// </summary>
        /// <param name="degRotation"></param>
        /// <returns></returns>
        public static float ToRadiants(float degRotation) => degRotation * DR;
        /// <summary>
        /// Convert radiants to degrees.
        /// </summary>
        /// <param name="radRotation">The rotation in radiants.</param>
        /// <returns>The input <paramref name="radRotation"/> in degrees.</returns>
        public static float ToDegrees(float radRotation) => radRotation * 57.2957795f;
        /// <summary>
        /// Finds the difference between two values.
        /// </summary>
        /// <param name="value1">The first value.</param>
        /// <param name="value2">The second value.</param>
        /// <returns></returns>
        public static float Diff(float value1, float value2) => System.Math.Abs(value1 - value2);
        /// <summary>
        /// Get the difference between two values as seen from a circle-plane contructed from the end and the start values.
        /// </summary>
        /// <param name="value1">The first value. Must fall within the range of <paramref name="start"/> and <paramref name="end"/>.</param>
        /// <param name="value2">The second value. Must fall within the range of <paramref name="start"/> and <paramref name="end"/>.</param>
        /// <param name="start">The </param>
        /// <param name="end"></param>
        /// <returns>The difference between two values as seen from a circle-plane contructed from the end and the start values.</returns>
        /// <exception cref="ArgumentException"></exception>
        public static float Diff(float value1, float value2, float start, float end)
        {
            ////if(value1 < start  || value1 > end) throw new ArgumentException(nameof(value1));
            ////if (value2 < start || value2 > end) throw new ArgumentException(nameof(value2));
            float dif = Diff(value2, value1);
            float dif2 = Diff((float)end, System.Math.Max(value2, value1)) + Diff((float)start, System.Math.Min(value2, value1)) + 1;
            //Console.WriteLine(dif + " :1 - 2: " + dif2);

            //return Math.Min(dif, dif2) * (Math.Min(dif, dif2) == dif2 ? -1 : 1f);

            return System.Math.Min(dif, dif2) * (System.Math.Min(value1, value2) == value2 ? -1 : 1f);
        }
        public static float Normalize(float rotation, bool radiants = false)
        {
            if (radiants) rotation = ToDegrees(rotation);
            float toret = MathF.Abs(rotation) % 360;
            if (rotation < 0)
            {
                toret = Diff(toret, 360);
                toret %= 360;
            }
            if (radiants) ToRadiants(toret);
            return toret;
        }
             
        //public static float Diff(float value1, float value2, Range range) => Diff(value1, value2, range.Start.Value, range.End.Value);
    }
}
