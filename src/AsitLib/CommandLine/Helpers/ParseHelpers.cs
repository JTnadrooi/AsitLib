using AsitLib.CommandLine.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AsitLib.CommandLine
{
    public static class ParseHelpers
    {
        public static string ParseSignature(ParameterInfo parameterInfo)
        {
            CustomSignatureAttribute? a = parameterInfo.GetCustomAttribute<CustomSignatureAttribute>();
            return a == null ? ParseSignature(parameterInfo.Name!) : (a.Name ?? parameterInfo.Name!);
        }
        public static string ParseSignature(MemberInfo memberInfo)
        {
            CustomSignatureAttribute? a = memberInfo.GetCustomAttribute<CustomSignatureAttribute>();
            return a == null ? ParseSignature(memberInfo.Name) : (a.Name ?? memberInfo.Name);
        }
        public static string ParseSignature(string signature) => Regex.Replace(signature, "(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z0-9])", "-$1", RegexOptions.Compiled).Trim().ToLower();

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

        public static ArgumentsInfo Parse(string args) => Parse(Split(args));
        public static ArgumentsInfo Parse(string[] args)
        {
            if (args.Length == 0) throw new ArgumentException("No command provided.", nameof(args));

            List<string> currentValues = new List<string>();
            List<Argument> arguments = new List<Argument>();
            string? currentName = null;
            int position = 0;
            bool noMoreParams = false;
            string token = string.Empty;

            void PushArgument()
            {
                arguments.Add(new Argument(new ArgumentTarget(currentName), currentValues.ToArray()));
                currentValues.Clear();
            }

            for (int i = 1; i < args.Length; i++)
            {
                token = args[i];

                if (!noMoreParams && token == "--")
                {
                    noMoreParams = true;
                    continue;
                }

                if (!noMoreParams && token.StartsWith("-"))
                {
                    if (currentName != null)
                    {
                        PushArgument();
                    }

                    currentName = token;
                }
                else
                {
                    if (currentName == null) arguments.Add(new Argument(new ArgumentTarget(position++), [token]));
                    else currentValues.Add(token);
                }
            }

            if (currentName != null) PushArgument();

            return new ArgumentsInfo(args[0], arguments.ToArray());
        }

        public static object?[] Conform(ArgumentsInfo info, ParameterInfo[] targets)
        {
            object?[] result = new object?[targets.Length];
            NullabilityInfoContext nullabilityInfoContext = new NullabilityInfoContext();
            HashSet<string> validTargets = new HashSet<string>();

            for (int i = 0; i < targets.Length; i++)
            {
                ParameterInfo target = targets[i];
                string targetName = ParseSignature(target);
                Argument? matchingArgument = null;
                NullabilityInfo nullabilityInfo = nullabilityInfoContext.Create(target);
                ShorthandAttribute? shorthandAttribute = target.GetCustomAttribute<ShorthandAttribute>();
                string? shortHandName = shorthandAttribute == null ? null : (shorthandAttribute.ShortHand ?? targetName[0].ToString());

                foreach (Argument arg in info.Arguments)
                {
                    if ((arg.Target.IsLongForm && arg.Target.SanitizedParameterToken == targetName) || (arg.Target.ParameterIndex == i) ||
                        (shortHandName != null && arg.Target.IsShortHand && arg.Target.SanitizedParameterToken == shortHandName))
                    {
                        matchingArgument = arg;
                        break;
                    }
                }

                if (matchingArgument == null) // no matching argument found.
                {
                    foreach (Argument arg in info.Arguments)
                    {
                        if (arg.Target.SanitizedParameterToken == $"no-{targetName}")
                        {
                            if (arg.Target.IsShortHand) throw new CommandException($"Shorthand anti-arguments are invalid.");
                            if (target.ParameterType != typeof(bool)) throw new CommandException($"Anti-arguments are only allowed for Boolean (true / false) parameters.");
                            if (target.GetCustomAttribute<AllowAntiArgumentAttribute>() == null) throw new CommandException($"An anti-argument is are not allowed by the '{targetName}' parameter.");
                            if (arg.Tokens.Length != 0) throw new CommandException("Anti-arguments cannot be passed any value.");

                            result[i] = false;
                            validTargets.Add(arg.Target.ToString());
                            goto Continue;
                        }
                    }
                    if (target.HasDefaultValue) // but has default value, so set default value.
                    {
                        result[i] = target.DefaultValue;
                        goto Continue;
                    }
                    if (nullabilityInfo.WriteState == NullabilityState.Nullable) // but can be null, so set null.
                    {
                        result[i] = null;
                        goto Continue;
                    }
                    throw new CommandException($"No matching value found for parameter '{targetName + (shortHandName == null ? string.Empty : $"(shorthand: {(shortHandName)})")}' (Index {i}).");
                }
                result[i] = Convert(matchingArgument.Value.Tokens, target.ParameterType);
                validTargets.Add(matchingArgument.Value.Target.ToString());
            Continue:;
            }

            foreach (Argument arg in info.Arguments)
            {
                if (!validTargets.Contains(arg.Target.ToString())) throw new CommandException($"No parameter found for argument target '{arg.Target}'");
            }

            return result;
        }

        public static object? Convert(string token, Type target) => Convert([token], target);
        public static object? Convert(string[] tokens, Type target)
        {
            if (tokens.Length == 0)
            {
                if (target == typeof(bool)) return true;
                else throw new InvalidOperationException($"Cannot convert empty token to '{target}' type.");
            }

            if (target.IsArray)
            {
                Type elementType = target.GetElementType()!;
                Array toretArray = Array.CreateInstance(elementType, tokens.Length);

                for (int i = 0; i < tokens.Length; i++) toretArray.SetValue(Convert([tokens[i]], elementType), i);

                return toretArray;
            }

            if (tokens.Length > 1) throw new InvalidOperationException($"Cannot convert multiple tokens to '{target}' type.");

            string token = tokens[0];

            if (target.IsEnum)
            {
                if (int.TryParse(token, out int result)) return Enum.ToObject(target, result);

                //Dictionary<string, string> names = ((IEnumerable<int>)Enum.GetValues(target))
                Dictionary<string, string> names = target
                    .GetFields(BindingFlags.Public | BindingFlags.Static)
                    .Select(f => new KeyValuePair<string, string>(ParseSignature(f), f.Name))
                    .ToDictionary();

                foreach (KeyValuePair<string, string> kvp in names)
                    if (string.Equals(kvp.Key, token, StringComparison.OrdinalIgnoreCase)) return Enum.Parse(target, kvp.Value);

                throw new ArgumentException($"Invalid enum value '{token}' could not be parsed to any of [{names.ToJoinedString(", ")}].", nameof(token));
            }

            return System.Convert.ChangeType(token, target);
        }
    }
}
