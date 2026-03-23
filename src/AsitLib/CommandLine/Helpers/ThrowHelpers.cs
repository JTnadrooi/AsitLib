using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib.CommandLine
{
    public static class ThrowHelpers
    {
        internal static IReadOnlyList<char> s_invalidChars = [
            '"', ',',
            '\0', '\a', '\b', '\t', '\n', '\v', '\f', '\r', '\x1B',
        ];

        internal static IReadOnlyList<char> s_invalidNameStartOrEndChars = [
            '-',
        ];

        internal static IReadOnlyList<Type> s_invalidOptionTypes = [
            typeof(void),
            typeof(DBNull),
        ];

        internal static void ThrowIfInvalidCommandProviderId(string id, [CallerArgumentExpression("id")] string? valueName = "Input")
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException($"'{valueName}' '{id}' is null or empty/whitespace.");
            if (id.Contains(' ')) throw new ArgumentException($"'{valueName}' '{id}' contains a space.");
            if (s_invalidNameStartOrEndChars.TryGetFirst(c => id.StartsOrEndsWith(c), out char startChar)) throw new ArgumentException($"{valueName} '{id}' starts with invalid character '{startChar}'.");
            if (s_invalidChars.TryGetFirst(c => id.Contains(c), out char invalidChar)) throw new ArgumentException($"{valueName} '{id}' contains invalid character '{invalidChar}'.");
        }

        internal static void ThrowIfInvalidCommandId(string id, [CallerArgumentExpression("id")] string? valueName = "Input")
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException($"'{valueName}' '{id}' is null or empty/whitespace.");
            if (s_invalidChars.TryGetFirst(c => id.Contains(c), out char invalidChar)) throw new ArgumentException($"{valueName} '{id}' contains invalid character '{invalidChar}'.");

            string[] parts = id.Split(' ');
            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i];

                if (part == string.Empty) throw new ArgumentException($"part in {valueName} '{id}' has invalid spaces.");

                if (i != 0)
                    if (s_invalidNameStartOrEndChars.TryGetFirst(c => part.StartsOrEndsWith(c), out char startChar)) throw new ArgumentException($"{valueName} '{part}' starts with invalid character '{startChar}'.");
            }
        }

        internal static void ThrowIfInvalidOptionType(Type type)
        {
            if (s_invalidOptionTypes.Contains(type)) throw new ArgumentException($"Invalid option type '{type}'.", "type");
        }

        internal static void ThrowIfInvalidOptionId(string id, [CallerArgumentExpression("id")] string? valueName = "Input")
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException($"'{valueName}' '{id}' is null or empty/whitespace.");
            if (s_invalidChars.TryGetFirst(c => id.Contains(c), out char invalidChar)) throw new ArgumentException($"{valueName} '{id}' contains invalid character '{invalidChar}'.");
            if (id.Contains(' ')) throw new ArgumentException($"{valueName} '{id}' contains invalid character '{invalidChar}'.");
            if (s_invalidNameStartOrEndChars.TryGetFirst(c => id.StartsWith(c), out char startChar)) throw new ArgumentException($"{valueName} '{id}' starts with invalid character '{startChar}'.");
        }
    }
}
