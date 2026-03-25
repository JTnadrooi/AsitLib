using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace AsitLib.CommandLine
{
    public static class ParseHelpers
    {
        //public static ImmutableArray<char> s_invalidChars = [
        //    '@', '<', '>', '.', '!', '#', '$',
        //    '%', '^', '&', '*', '(', ')',
        //    '+', '=', '{', '}', '[', ']',
        //    '|', '\\', '/', ':', ';', '"', '\'',
        //    ',', '`', '~',
        //    '\0', '\a', '\b', '\t', '\n', '\v', '\f', '\r', '\x1B',
        //];

        internal static ImmutableArray<string> s_antiPrefixes = [
            "no"
        ];

        /// <summary>
        /// Determines whether the specified <paramref name="signature"/> is valid as a generic flag.
        /// </summary>
        /// <returns><see langword="true"/> if the signature is valid as a generic flag; otherwise, <see langword="false"/>.</returns>
        public static bool IsValidGenericFlagCall(string signature)
        {
            string sanitized = signature.TrimStart('-');
            return (sanitized.Length == 1 && signature.Length - sanitized.Length == 1) || (sanitized.Length > 1 && signature.Length - sanitized.Length == 2);
        }

        public static string GetGenericFlagSignature(string signature)
        {
            if (IsValidGenericFlagCall(signature)) throw new InvalidOperationException("'signature' is already a valid generic flag signature.");

            if (signature.Length == 1) return "-" + signature;
            else return "--" + signature;
        }

        public static string GetSignature(ParameterInfo parameterInfo)
        {
            if (parameterInfo.Name is null) throw new InvalidOperationException("Cannot get signature from return parameter.");

            SignatureAttribute? a = parameterInfo.GetCustomAttribute<SignatureAttribute>();
            return a is null ? GetSignature(parameterInfo.Name) : a.Name;
        }

        public static string GetSignature(MemberInfo memberInfo)
        {
            SignatureAttribute? a = memberInfo.GetCustomAttribute<SignatureAttribute>();
            return a is null ? GetSignature(memberInfo.Name) : a.Name;
        }

        public static string GetSignature(string str) => Regex.Replace(str, "(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z0-9])", "-$1", RegexOptions.Compiled).Trim().ToLower();

        public static string[] GetTokens(string str)
        {
            List<string> result = new List<string>();
            StringBuilder sb = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];
                bool escaped = i > 0 && str[i - 1] == '\\';

                if (c == '"')
                {
                    if (!escaped)
                    {
                        inQuotes = !inQuotes;

                        sb.Append(c);
                        continue;
                    }
                    else if (sb.Length > 0 && sb[^1] == '\\') sb.Remove(sb.Length - 1, 1);
                }
                else if (char.IsWhiteSpace(c) && !inQuotes)
                {
                    if (sb.Length > 0)
                    {
                        result.Add(sb.ToString());
                        sb.Clear();
                    }
                    continue;
                }

                if (c != '"' || escaped)
                {
                    sb.Append(c);
                }
            }

            if (sb.Length > 0) result.Add(sb.ToString());

            return result.ToArray();
        }

        public static bool IsQuoted(string str)
        {
            return str != UnQuote(str);
        }

        public static string UnQuote(string str)
        {
            ArgumentException.ThrowIfNullOrEmpty(nameof(str));

            const char magicChar = '\u001F'; // escaped quotes will be made to be the magicChar

            if (str.Contains(magicChar))
                throw new ArgumentException("String cannot contain magic character 001F.", nameof(str));

            string replacedString = str.Replace("\\\"", magicChar.ToString()); // /" -> <magicchar>

            if (replacedString.Contains('"'))
            {
                bool startsWithQuote = replacedString[0] == '"';
                bool endsWithQuote = replacedString[^1] == '"';

                if (replacedString.Length == 1)
                {
                    throw new ArgumentException($"Mismatched quotes in string.", nameof(str));
                }

                if (startsWithQuote ^ endsWithQuote)
                {
                    throw new ArgumentException($"Mismatched quotes in string.", nameof(str));
                }
            }

            if (replacedString.Length >= 2 && str[0] == '"' && str[^1] == '"')
            {
                return str.Substring(1, str.Length - 2);
            }

            return str;
        }

        public static string[] GetAntiIds(string[] ids)
        {
            IEnumerable<string> result = Enumerable.Empty<string>();
            foreach (string prefix in s_antiPrefixes)
            {
                result = result.Concat(ids.Select(id => $"{prefix}-{id}"));
            }
            return result.ToArray();
        }

        /// <summary>
        /// Extracts the <see cref="GlobalOption"/> instances that listen to the specified <paramref name="call"/>.
        /// </summary>
        /// <param name="call">The call information containing arguments to evaluate. This parameter is updated to remove arguments that target global options.</param>
        /// <param name="globalOptions">The array of global option handlers to check against the call arguments.</param>
        /// <returns>An array of unique <see cref="GlobalOption"/> instances that are targeted by the named arguments in the call.</returns>
        public static GlobalOption[] ExtractGlobalOptions(ref CallInfo call, ReadOnlySpan<GlobalOption> globalOptions)
        {
            HashSet<GlobalOption> result = new HashSet<GlobalOption>();
            HashSet<Argument> validArguments = new HashSet<Argument>();

            foreach (Argument arg in call.Arguments.Where(a => a.Target.Id is not null))
                for (int i = 0; i < globalOptions.Length; i++)
                {
                    if (arg.Target.IsMatchFor(globalOptions[i]))
                        if (!validArguments.Add(arg) || !result.Add(globalOptions[i]))
                        {
                            throw new CommandArgumentException("Duplicate argument to GlobalOption mapping.");
                        }
                }

            call = new CallInfo(call.CommandId, call.Arguments.Except(validArguments).ToArray());
            return result.ToArray();
        }

        /// <summary>
        /// Casts the <paramref name="call"/> arguments to the specified options. Casting is done through <see cref="OptionInfo.Conform(ReadOnlySpan{string})"/>.
        /// </summary>
        /// <param name="options">The array of <see cref="OptionInfo"/> instances to conform the <see cref="CallInfo.Arguments"/> against.</param>
        /// <returns>An array of values conformed to the specified options, in the same order as the <paramref name="options"/> array.</returns>
        public static object?[] Conform(ref CallInfo call, ReadOnlySpan<OptionInfo> options)
        {
            object?[] result = new object?[options.Length];
            NullabilityInfoContext nullabilityInfoContext = new NullabilityInfoContext();
            HashSet<Argument> validArguments = new HashSet<Argument>();

            for (int i = 0; i < options.Length; i++)
            {
                OptionInfo option = options[i];
                Argument? matchingArgument = null;

                foreach (Argument arg in call.Arguments)
                    if (arg.Target.IsMatchFor(option, i))
                    {
                        if (matchingArgument is not null)
                            throw new CommandArgumentException($"Duplicate argument found for target '{option.Id}'."); // still needed if positional and named arguments conflict.
                        matchingArgument = arg;
                    }

                if (matchingArgument is null) // no matching argument found.
                {
                    if (option.HasDefaultValue) // but has default value, so set default value.
                    {
                        result[i] = option.DefaultValue;
                        goto Continue;
                    }
                    else throw new CommandArgumentException($"No matching value found for parameter '{option}'.");
                }

                if (matchingArgument.Target.IsAntiTarget)
                    result[i] = option.GetAntiTargetValue();
                else
                    result[i] = option.Conform(matchingArgument.Tokens.AsSpan());

                validArguments.Add(matchingArgument);

            Continue:;
            }

            call = new CallInfo(call.CommandId, call.Arguments.Except(validArguments).ToArray());

            return result;
        }
    }
}
