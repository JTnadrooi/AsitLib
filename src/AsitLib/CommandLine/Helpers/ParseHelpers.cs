using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace AsitLib.CommandLine
{
    public static class ParseHelpers
    {
        //public static IReadOnlyList<char> s_invalidChars = [
        //    '@', '<', '>', '.', '!', '#', '$',
        //    '%', '^', '&', '*', '(', ')',
        //    '+', '=', '{', '}', '[', ']',
        //    '|', '\\', '/', ':', ';', '"', '\'',
        //    ',', '`', '~',
        //    '\0', '\a', '\b', '\t', '\n', '\v', '\f', '\r', '\x1B',
        //];

        internal static IReadOnlyList<char> s_invalidChars = [
            '"', '\'', ',', '!',
            '\0', '\a', '\b', '\t', '\n', '\v', '\f', '\r', '\x1B',
        ];

        internal static IReadOnlyList<char> s_invalidStartChars = [
            '-', ' ',
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

        internal static void ThrowIfInvalidName(string name, bool allowSpace, [CallerArgumentExpression("name")] string? valueName = "Input")
        {
            if (string.IsNullOrWhiteSpace(name)) throw new InvalidOperationException($"'{valueName}' '{name}' is null or empty/whitespace.");
            if (!allowSpace && name.Contains(' ')) throw new InvalidOperationException($"'{valueName}' '{name}' contains a space.");
            if (name == string.Empty) throw new InvalidOperationException($"'{valueName}' is an empty string.");

            foreach (string part in name.Split(' '))
            {
                if (part == string.Empty) throw new InvalidOperationException($"{valueName} '{name}' has invalid spaces.");
                if (s_invalidChars.TryGetFirst(c => part.Contains(c), out char containedChar)) throw new InvalidOperationException($"{valueName} '{name}' contains invalid character '{containedChar}'.");
                if (s_invalidStartChars.TryGetFirst(c => part.StartsWith(c), out char startChar)) throw new InvalidOperationException($"{valueName} '{name}' starts with invalid character '{startChar}'.");
                if (part.StartsWith('-') || part.EndsWith('-')) throw new InvalidOperationException($"{valueName} '{name}' cannot start or end with a dash.");
            }
        }


        public static string[] SplitWithRespectForQuotes(string str)
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

                sb.Append(c);
            }

            if (sb.Length > 0) result.Add(sb.ToString());

            return result.ToArray();
        }

        /// <summary>
        /// Extracts the <see cref="GlobalOption"/> instances that listen to the specified <paramref name="call"/>.
        /// </summary>
        /// <param name="call">The call information containing arguments to evaluate. This parameter is updated to remove arguments that target global options.</param>
        /// <param name="globalOptions">The array of global option handlers to check against the call arguments.</param>
        /// <returns>An array of unique <see cref="GlobalOption"/> instances that are targeted by the named arguments in the call.</returns>
        public static GlobalOption[] ExtractGlobalOptions(ref CallInfo call, GlobalOption[] globalOptions)
        {
            HashSet<GlobalOption> result = new HashSet<GlobalOption>();
            HashSet<Argument> validArguments = new HashSet<Argument>();

            foreach (Argument arg in call.Arguments.Where(a => a.Target.UsesExplicitName))
                foreach (GlobalOption flagHandler in globalOptions)
                    if (arg.Target.TargetsFlag(flagHandler))
                        if (!validArguments.Add(arg) || !result.Add(flagHandler))
                        {
                            throw new InvalidOperationException("Duplicate argument to flag mapping.");
                        }

            call = new CallInfo(call.CommandId, call.Arguments.Except(validArguments).ToList(), call.CallsGenericFlag);
            return result.ToArray();
        }

        /// <summary>
        /// Casts the <paramref name="call"/> arguments to the specified options. Casting is done through <see cref="OptionInfo.GetValue(IReadOnlyList{string})"/>.
        /// </summary>
        /// <param name="options">The array of <see cref="OptionInfo"/> instances to conform the <see cref="CallInfo.Arguments"/> against.</param>
        /// <param name="context">The command context, used for option inheritance policies.</param>
        /// <returns>An array of values conformed to the specified options, in the same order as the <paramref name="options"/> array.</returns>
        public static object?[] Conform(ref CallInfo call, OptionInfo[] options, CommandContext? context = null)
        {
            object?[] result = new object?[options.Length];
            NullabilityInfoContext nullabilityInfoContext = new NullabilityInfoContext();
            HashSet<Argument> validArguments = new HashSet<Argument>();

            for (int i = 0; i < options.Length; i++)
            {
                OptionInfo option = options[i];
                Argument? matchingArgument = null;
                string? shortHandName = option.Shorthand;

                option.ThrowExceptionIfNoName();

                foreach (Argument arg in call.Arguments)
                    if ((arg.Target.OptionIndex == i) // positional.
                        || (arg.Target.IsLongForm && arg.Target.SanitizedOptionToken == option.Name && option.GetInheritedPassingPoliciesFromContext(context).HasFlag(OptionPassingPolicies.Named)) // longform.
                        || (shortHandName is not null && arg.Target.IsShorthand && arg.Target.SanitizedOptionToken == shortHandName && option.PassingPolicies.HasFlag(OptionPassingPolicies.Named))) // shorthand.
                    {
                        if (matchingArgument is not null) throw new CommandException($"Duplicate argument found for target '{option.Name}'.");
                        matchingArgument = arg;
                    }

                if (option.AntiParameterName is not null)
                    foreach (Argument arg in call.Arguments)
                        if (arg.Target.UsesExplicitName && arg.Target.SanitizedOptionToken == (option.AntiParameterName ?? $"no-{option.Name}"))
                        {
                            if (arg.Target.IsShorthand) throw new CommandException($"Shorthand anti-arguments are invalid.");
                            if (option.OptionType != typeof(bool)) throw new CommandException($"Anti-arguments are only allowed for Boolean (true / false) parameters.");
                            if (matchingArgument is not null) throw new CommandException($"Duplicate argument found for target '{option.Name}'.");
                            if (arg.Tokens.Count != 0) throw new CommandException("Anti-arguments cannot be passed any value.");

                            result[i] = false;
                            validArguments.Add(arg);
                            goto Continue;
                        }

                if (matchingArgument is null) // no matching argument found.
                {
                    if (option.HasDefaultValue) // but has default value, so set default value.
                    {
                        result[i] = option.DefaultValue;
                        goto Continue;
                    }
                    else throw new CommandException($"No matching value found for parameter '{option.Name + (shortHandName is null ? string.Empty : $"(shorthand: {(shortHandName)})")}' (Index {i}).");
                }

                result[i] = option.GetValue(matchingArgument.Tokens);
                validArguments.Add(matchingArgument);

            Continue:;
            }

            call = new CallInfo(call.CommandId, call.Arguments.Except(validArguments).ToList(), call.CallsGenericFlag);

            return result;
        }

        #region CASTING

        public static object? GetValue(string token, Type target) => GetValue([token], target);
        public static object? GetValue(string token, OptionInfo target) => GetValue([token], target);
        public static object? GetValue(IReadOnlyList<string> tokens, Type target) => GetValue(tokens, OptionInfo.FromType(target));
        public static object? GetValue(IReadOnlyList<string> tokens, OptionInfo target)
        {
            object? GetValuePrivate()
            {
                if (tokens.Count == 0)
                {
                    if (target.ImplicitValue is not null) return target.ImplicitValue;
                    if (target.OptionType == typeof(bool)) return true;
                    else throw new InvalidOperationException($"Cannot convert empty token to '{target}' type.");
                }

                if (target.OptionType.IsArray)
                {
                    Type elementType = target.OptionType.GetElementType()!;
                    Array toretArray = Array.CreateInstance(elementType, tokens.Count);

                    for (int i = 0; i < tokens.Count; i++) toretArray.SetValue(GetValue([tokens[i]], elementType), i);

                    return toretArray;
                }

                if (tokens.Count > 1) throw new InvalidOperationException($"Cannot convert multiple tokens to '{target}' type.");

                string token = tokens[0];

                if (target.OptionType.IsEnum)
                {
                    if (int.TryParse(token, out int result)) return Enum.ToObject(target.OptionType, result);

                    Dictionary<string, string> names = target.OptionType
                        .GetFields(BindingFlags.Public | BindingFlags.Static)
                        .Select(f => new KeyValuePair<string, string>(GetSignature(f), f.Name))
                        .ToDictionary();

                    foreach (KeyValuePair<string, string> kvp in names)
                        if (string.Equals(kvp.Key, token, StringComparison.OrdinalIgnoreCase)) return Enum.Parse(target.OptionType, kvp.Value);

                    throw new ArgumentException($"Invalid enum value '{token}' could not be parsed to any of [{names.ToJoinedString(", ")}].", nameof(token));
                }

                return System.Convert.ChangeType(token, target.OptionType);
            }

            object? toret = GetValuePrivate();

            target.ThrowExceptionIfInvalidValue(toret);

            return toret;
        }

        #endregion
    }
}
