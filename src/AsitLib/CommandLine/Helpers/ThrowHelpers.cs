using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib.CommandLine
{
    internal static class ThrowHelpers
    {
        private static ImmutableArray<char> s_invalidChars = [
            '"', ',',
            '\0', '\a', '\b', '\t', '\n', '\v', '\f', '\r', '\x1B',
        ];

        private static ImmutableArray<char> s_invalidNameStartOrEndChars = [
            '-',
        ];

        private static ImmutableArray<Type> s_invalidOptionTypes = [
            typeof(void),
            typeof(DBNull),
        ];

        public static void ThrowIfInvalidCommandProviderId(string id, [CallerArgumentExpression("id")] string? valueName = "Input")
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException($"'{valueName}' '{id}' is null or empty/whitespace.");
            if (id.Contains(' ')) throw new ArgumentException($"'{valueName}' '{id}' contains a space.");
            if (s_invalidNameStartOrEndChars.TryGetFirst(c => id.StartsOrEndsWith(c), out char startChar)) throw new ArgumentException($"{valueName} '{id}' starts with invalid character '{startChar}'.");
            if (s_invalidChars.TryGetFirst(c => id.Contains(c), out char invalidChar)) throw new ArgumentException($"{valueName} '{id}' contains invalid character '{invalidChar}'.");
        }

        public static void ThrowIfInvalidCommandId(string id, [CallerArgumentExpression("id")] string? valueName = "Input")
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException($"'{valueName}' '{id}' is null or empty/whitespace.");
            if (s_invalidChars.TryGetFirst(c => id.Contains(c), out char invalidChar)) throw new ArgumentException($"{valueName} '{id}' contains invalid character '{invalidChar}'.");

            string[] parts = id.Split(' ');
            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i];

                if (part == string.Empty) throw new ArgumentException($"part in {valueName} '{id}' has invalid spaces.");

                if (parts.Length != 1)
                    if (s_invalidNameStartOrEndChars.TryGetFirst(c => part.StartsOrEndsWith(c), out char startChar)) throw new ArgumentException($"{valueName} '{part}' starts with invalid character '{startChar}'.");
            }
        }

        public static void ThrowIfInvalidOptionType(Type type)
        {
            if (s_invalidOptionTypes.Contains(type)) throw new ArgumentException($"Invalid option type '{type}'.", "type");
        }

        public static void ThrowIfInvalidOptionId(string id, [CallerArgumentExpression("id")] string? valueName = "Input")
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException($"'{valueName}' '{id}' is null or empty/whitespace.");
            if (s_invalidChars.TryGetFirst(c => id.Contains(c), out char invalidChar)) throw new ArgumentException($"{valueName} '{id}' contains invalid character '{invalidChar}'.");
            if (id.Contains(' ')) throw new ArgumentException($"{valueName} '{id}' contains invalid character '{invalidChar}'.");
            if (s_invalidNameStartOrEndChars.TryGetFirst(c => id.StartsWith(c), out char startChar)) throw new ArgumentException($"{valueName} '{id}' starts with invalid character '{startChar}'.");
        }
    }
}
