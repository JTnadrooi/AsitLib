#nullable enable

namespace AsitLib.Numerics
{
    /// <summary>
    /// Provides mathematical operations.
    /// </summary>
    public static class MathAL
    {
        /// <summary>
        /// Inverts a number using a reference point. 
        /// The result is the mirrored value from the specified point.
        /// </summary>
        /// <param name="value">The value to invert.</param>
        /// <param name="pointZero">The reference point to invert around (defaults to 0).</param>
        /// <returns>The inverted value.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the value is not a normal number.</exception>
        public static float Invert(float value, float pointZero = 0f)
        {
            if (float.IsNaN(value) || value == float.PositiveInfinity || value == float.NegativeInfinity)
                throw new InvalidOperationException("Value is not a normal number.");

            if (value == pointZero) return value;

            float dif = Math.Abs(pointZero - value);
            return value + (dif * 2 * Math.Sign(pointZero - value));
        }

        /// <summary>
        /// Normalizes an angle to the range [0, 360) for degrees or [-180, 180) for radians.
        /// </summary>
        /// <param name="rotation">The rotation value to normalize.</param>
        /// <param name="isRadians">Indicates whether the rotation value is in radians (defaults to <c>false</c>, meaning degrees).</param>
        /// <returns>The normalized rotation value.</returns>
        public static float Normalize(float rotation, bool isRadians = false)
        {
            if (isRadians) rotation = float.RadiansToDegrees(rotation);
            float normalized = Math.Abs(rotation) % 360;
            if (rotation < 0)
            {
                normalized = 360 - normalized;
                normalized %= 360;
            }
            return isRadians ? float.DegreesToRadians(normalized) : normalized;
        }

        /// <summary>
        /// Gets the absolute difference between two float values.
        /// </summary>
        /// <param name="value1">The first value.</param>
        /// <param name="value2">The second value.</param>
        /// <returns>The absolute difference between the values.</returns>
        public static float Difference(float value1, float value2) => Math.Abs(value1 - value2);

        /// <summary>
        /// Averages a set of float values by calculating their sum and dividing by the number of values.
        /// </summary>
        /// <param name="values">The values to average.</param>
        /// <returns>The average of the values.</returns>
        /// <exception cref="InvalidOperationException">Thrown if any value is not a normal number.</exception>
        public static float Average(params float[] values)
        {
            float total = values.Sum();
            if (float.IsNaN(total) || total == float.PositiveInfinity || total == float.NegativeInfinity)
                throw new InvalidOperationException("Value is not a normal number; average failed.");
            return total / values.Length;
        }
    }
}
