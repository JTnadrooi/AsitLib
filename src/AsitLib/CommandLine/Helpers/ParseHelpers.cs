using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace AsitLib.CommandLine
{
    public static class ParseHelpers
    {
        public static IReadOnlyList<char> s_invalidChars = [
            '@', '<', '>', '.', '!', '#', '$',
            '%', '^', '&', '*', '(', ')',
            '+', '=', '{', '}', '[', ']',
            '|', '\\', '/', ':', ';', '"', '\'',
            ',', '`', '~',
            '\0', '\a', '\b', '\t', '\n', '\v', '\f', '\r', '\x1B',
        ];

        public static IReadOnlyList<char> s_invalidStartChars = [
            '-', ' ',
        ];

        public static bool IsValidGenericFlagCall(string signature) // maybe make signature struct
        {
            string sanitized = signature.TrimStart('-');
            return (sanitized.Length == 1 && signature.Length - sanitized.Length == 1) || (sanitized.Length > 1 && signature.Length - sanitized.Length == 2);
        }

        public static string GetGenericFlagSignature(string signature) // maybe make signature struct
        {
            if (IsValidGenericFlagCall(signature)) throw new InvalidOperationException("'signature' is already a valid generic flag signature.");

            if (signature.Length == 1) return "-" + signature;
            else return "--" + signature;
        }

        public static string GetSignature(ParameterInfo parameterInfo)
        {
            if (parameterInfo.Name == null) throw new InvalidOperationException("Cannot get signature from return parameter.");

            SignatureAttribute? a = parameterInfo.GetCustomAttribute<SignatureAttribute>();
            return a is null ? GetSignature(parameterInfo.Name) : a.Name;
        }

        public static string GetSignature(MemberInfo memberInfo)
        {
            SignatureAttribute? a = memberInfo.GetCustomAttribute<SignatureAttribute>();
            return a is null ? GetSignature(memberInfo.Name) : a.Name;
        }

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

        public static string GetSignature(string str) => Regex.Replace(str, "(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z0-9])", "-$1", RegexOptions.Compiled).Trim().ToLower();

        public static string[] Split(string str)
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

        public static GlobalOption[] ExtractFlags(ref ArgumentsInfo argsInfo, GlobalOption[] flagHandlers)
        {
            HashSet<GlobalOption> toret = new HashSet<GlobalOption>();
            HashSet<Argument> validArguments = new HashSet<Argument>();

            foreach (Argument arg in argsInfo.Arguments.Where(a => a.Target.UsesExplicitName))
                foreach (GlobalOption flagHandler in flagHandlers)
                    if (arg.Target.TargetsFlag(flagHandler))
                        if (!validArguments.Add(arg) || !toret.Add(flagHandler))
                        {
                            throw new Exception();
                        }

            argsInfo = new ArgumentsInfo(argsInfo.CommandId, argsInfo.Arguments.Except(validArguments).ToList(), argsInfo.CallsGenericFlag);
            return toret.ToArray();
        }

        public static object?[] Conform(ref ArgumentsInfo argsInfo, OptionInfo[] options, CommandContext? context = null)
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

                foreach (Argument arg in argsInfo.Arguments)
                    if ((arg.Target.OptionIndex == i) // positional.
                        || (arg.Target.IsLongForm && arg.Target.SanitizedOptionToken == option.Name && option.GetInheritedPassingPoliciesFromContext(context).HasFlag(OptionPassingPolicies.Named)) // longform.
                        || (shortHandName is not null && arg.Target.IsShorthand && arg.Target.SanitizedOptionToken == shortHandName && option.PassingPolicies.HasFlag(OptionPassingPolicies.Named))) // shorthand.
                    {
                        if (matchingArgument is not null) throw new CommandException($"Duplicate argument found for target '{option.Name}'.");
                        matchingArgument = arg;
                    }

                if (option.AntiParameterName is not null)
                    foreach (Argument arg in argsInfo.Arguments)
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
                    if (option.OptionType == typeof(CommandContext))
                    {
                        result[i] = context ?? throw new CommandException("Cannot inject engine when engine is not passed to Conform function.");
                        goto Continue;
                    }
                    else if (option.HasDefaultValue) // but has default value, so set default value.
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

            argsInfo = new ArgumentsInfo(argsInfo.CommandId, argsInfo.Arguments.Except(validArguments).ToList(), argsInfo.CallsGenericFlag);

            return result;
        }

        public static object? GetValue(string token, Type target) => GetValue([token], target);
        public static object? GetValue(string token, OptionInfo target) => GetValue([token], target);
        public static object? GetValue(IReadOnlyList<string> tokens, Type target) => GetValue(tokens, new OptionInfo("__noname__", target));
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
    }
}
