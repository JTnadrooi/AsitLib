using AsitLib.CommandLine.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.Versioning;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AsitLib.CommandLine
{
    public static class ParseHelpers
    {
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
            string commandId = args[0];
            bool callsGenericFlag = commandId.StartsWith("--");

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
                    if (currentName is not null)
                    {
                        PushArgument();
                    }

                    currentName = token;
                }
                else
                {
                    if (currentName is null) arguments.Add(new Argument(new ArgumentTarget(position++), [token]));
                    else currentValues.Add(token);
                }
            }

            if (currentName is not null) PushArgument();


            return new ArgumentsInfo(args[0], arguments.ToArray(), callsGenericFlag);
        }

        public static GlobalOptionHandler[] ExtractFlags(ref ArgumentsInfo argsInfo, GlobalOptionHandler[] flagHandlers)
        {
            HashSet<GlobalOptionHandler> toret = new HashSet<GlobalOptionHandler>();
            HashSet<Argument> validArguments = new HashSet<Argument>();

            foreach (Argument arg in argsInfo.Arguments.Where(a => a.Target.UsesExplicitName))
                foreach (GlobalOptionHandler flagHandler in flagHandlers)
                    if (arg.Target.TargetsFlag(flagHandler))
                        if (!validArguments.Add(arg) || !toret.Add(flagHandler))
                        {
                            throw new Exception();
                        }

            argsInfo = new ArgumentsInfo(argsInfo.CommandId, argsInfo.Arguments.Except(validArguments).ToList(), argsInfo.CallsGenericFlag);
            return toret.ToArray();
        }

        public static object?[] Conform(ref ArgumentsInfo argsInfo, OptionInfo[] targets)
        {
            object?[] result = new object?[targets.Length];
            NullabilityInfoContext nullabilityInfoContext = new NullabilityInfoContext();
            HashSet<Argument> validArguments = new HashSet<Argument>();

            for (int i = 0; i < targets.Length; i++)
            {
                OptionInfo target = targets[i];
                Argument? matchingArgument = null;
                ShorthandAttribute? shorthandAttribute = target.GetAttribute<ShorthandAttribute>();
                AllowAntiArgumentAttribute? allowAntiArgumentAttribute = target.GetAttribute<AllowAntiArgumentAttribute>();
                string? shortHandName = shorthandAttribute is null ? null : (shorthandAttribute.Shorthand ?? target.Name[0].ToString());

                foreach (Argument arg in argsInfo.Arguments)
                {
                    if ((arg.Target.IsLongForm && arg.Target.SanitizedOptionToken == target.Name) || (arg.Target.OptionIndex == i) ||
                        (shortHandName is not null && arg.Target.IsShorthand && arg.Target.SanitizedOptionToken == shortHandName))
                    {
                        if (matchingArgument is not null) throw new CommandException($"Duplicate argument found for target '{target.Name}'.");
                        matchingArgument = arg;
                        //break;
                    }
                }

                if (allowAntiArgumentAttribute is not null)
                    foreach (Argument arg in argsInfo.Arguments)
                    {
                        if (arg.Target.UsesExplicitName && arg.Target.SanitizedOptionToken == (allowAntiArgumentAttribute.Name ?? $"no-{target.Name}"))
                        {
                            if (arg.Target.IsShorthand) throw new CommandException($"Shorthand anti-arguments are invalid.");
                            if (target.Type != typeof(bool)) throw new CommandException($"Anti-arguments are only allowed for Boolean (true / false) parameters.");
                            if (matchingArgument is not null) throw new CommandException($"Duplicate argument found for target '{target.Name}'.");
                            if (arg.Tokens.Count != 0) throw new CommandException("Anti-arguments cannot be passed any value.");

                            result[i] = false;
                            validArguments.Add(arg);
                            goto Continue;
                        }
                    }

                if (matchingArgument is null) // no matching argument found.
                {
                    if (target.HasDefaultValue) // but has default value, so set default value.
                    {
                        result[i] = target.DefaultValue;
                        goto Continue;
                    }
                    if (target.WriteState == NullabilityState.Nullable) // but can be null, so set null.
                    {
                        result[i] = null;
                        goto Continue;
                    }

                    //if (matchingArgument.Value.Target.UsesExplicitName.) throw new CommandException($"An anti-argument is are not allowed by the '{targetName}' parameter.");

                    throw new CommandException($"No matching value found for parameter '{target.Name + (shortHandName is null ? string.Empty : $"(shorthand: {(shortHandName)})")}' (Index {i}).");
                }

                result[i] = Convert(matchingArgument.Tokens, target.Type, target.Attributes);
                validArguments.Add(matchingArgument);
            Continue:;
            }

            argsInfo = new ArgumentsInfo(argsInfo.CommandId, argsInfo.Arguments.Except(validArguments).ToList(), argsInfo.CallsGenericFlag);

            return result;
        }

        public static object? Convert(string token, Type target, IEnumerable<object>? attributes = null) => Convert([token], target, attributes);
        public static object? Convert(IReadOnlyList<string> tokens, Type target, IEnumerable<object>? attributes = null)
        {
            ImplicitValueAttribute? implicitValueAttribute = null;
            List<ValidationAttribute> validationAttributes = new List<ValidationAttribute>();

            foreach (object attribute in attributes ?? Enumerable.Empty<object>())
                switch (attribute)
                {
                    case ValidationAttribute a:
                        validationAttributes.Add(a);
                        break;
                    case ImplicitValueAttribute a:
                        implicitValueAttribute = a;
                        break;
                }

            object? ConvertPrivate()
            {
                if (tokens.Count == 0)
                {
                    if (implicitValueAttribute is not null) return implicitValueAttribute.Value;
                    if (target == typeof(bool)) return true;
                    else throw new InvalidOperationException($"Cannot convert empty token to '{target}' type.");
                }

                if (target.IsArray)
                {
                    Type elementType = target.GetElementType()!;
                    Array toretArray = Array.CreateInstance(elementType, tokens.Count);

                    for (int i = 0; i < tokens.Count; i++) toretArray.SetValue(Convert([tokens[i]], elementType), i);

                    return toretArray;
                }

                if (tokens.Count > 1) throw new InvalidOperationException($"Cannot convert multiple tokens to '{target}' type.");

                string token = tokens[0];

                if (target.IsEnum)
                {
                    if (int.TryParse(token, out int result)) return Enum.ToObject(target, result);

                    Dictionary<string, string> names = target
                        .GetFields(BindingFlags.Public | BindingFlags.Static)
                        .Select(f => new KeyValuePair<string, string>(GetSignature(f), f.Name))
                        .ToDictionary();

                    foreach (KeyValuePair<string, string> kvp in names)
                        if (string.Equals(kvp.Key, token, StringComparison.OrdinalIgnoreCase)) return Enum.Parse(target, kvp.Value);

                    throw new ArgumentException($"Invalid enum value '{token}' could not be parsed to any of [{names.ToJoinedString(", ")}].", nameof(token));
                }

                return System.Convert.ChangeType(token, target);
            }

            object? toret = ConvertPrivate();

            foreach (ValidationAttribute attribute in validationAttributes)
            {
                ValidationResult? result = attribute.GetValidationResult(toret, new ValidationContext(toret!) { DisplayName = "INPUT" });
                if (result != ValidationResult.Success) throw new CommandException($"Argument value '{toret}' is invalid: {result!.ErrorMessage}");
            }

            return toret;
        }
    }
}
