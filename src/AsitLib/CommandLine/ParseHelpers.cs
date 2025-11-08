using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AsitLib.CommandLine
{
    public readonly struct ArgumentTarget
    {
        public readonly string? ParameterToken;
        public readonly int? ParameterIndex;
        public readonly bool ShortHand => UsesExplicitName && !ParameterToken!.StartsWith("--");
        public readonly string? SanitizedParameterToken => ParameterToken?.TrimStart('-');

        public bool UsesExplicitName => ParameterToken != null;

        public ArgumentTarget(string parameterName)
        {
            ParameterToken = parameterName;
            ParameterIndex = null;
        }

        public ArgumentTarget(int parameterIndex)
        {
            ParameterToken = null;
            ParameterIndex = parameterIndex;
        }

        public override string ToString() => UsesExplicitName ? ParameterToken! : ParameterIndex!.ToString()!;

    }

    public readonly struct Argument
    {
        public readonly ArgumentTarget Target;
        public readonly string[] Tokens;

        public Argument(ArgumentTarget target, string[] tokens)
        {
            Target = target;
            Tokens = tokens;
        }

        public override string ToString() => $"{{Target: '{Target}', Tokens: [{Tokens.ToJoinedString(", ")}]}}";
    }

    public readonly struct ArgumentsInfo
    {
        public readonly string CommandId;
        public readonly Argument[] Arguments;

        public ArgumentsInfo(string commandId, Argument[] arguments)
        {
            CommandId = commandId;
            Arguments = arguments;
        }

        public string ToDisplayString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("ArgumentsInfo:");
            sb.AppendLine($"\tCommandId: {this.CommandId}");
            sb.AppendLine("\tArguments:");

            for (var i = 0; i < this.Arguments.Length; i++)
            {
                Argument arg = this.Arguments[i];
                sb.AppendLine($"\t\tTarget: {arg.Target}");
                sb.AppendLine($"\t\tShortHand: {arg.Target.ShortHand}");
                sb.AppendLine("\t\tTokens:");
                foreach (string token in arg.Tokens) sb.AppendLine($"\t\t\t{token}");
            }

            return sb.ToString();
        }

        public override string ToString() => $"{{Id: '{CommandId}', Expected parameters: [{Arguments.Select(a => a.Target).ToJoinedString(", ")}]}}";
    }

    public static class CommandHelpers
    {
        public static string CreateCommandId(CommandAttribute attribute, CommandProvider provider, MethodInfo methodInfo)
        {
            if (methodInfo.Name == "_M") return provider.Namespace;
            else return (attribute.InheritNamespace ? (provider.Namespace + "-") : string.Empty) + ParseHelpers.ParseSignature(attribute.Id ?? methodInfo.Name.ToLower());
        }
    }

    public static class ParseHelpers
    {
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
                arguments.Add(new Argument(new ArgumentTarget(currentName), (currentValues.Count == 0 ? ["true"] : currentValues.ToArray())));
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
                        currentValues.Clear();
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

            for (int i = 0; i < targets.Length; i++)
            {
                ParameterInfo target = targets[i];
                Argument? matchingArgument = null;

                // try to find the matching argument
                foreach (Argument arg in info.Arguments)
                {
                    if (arg.Target.ParameterIndex == i || (arg.Target.SanitizedParameterToken == target.Name && arg.Target.UsesExplicitName))
                    {
                        matchingArgument = arg;
                        break;
                    }
                }

                if (matchingArgument == null) // no matching argument found
                {
                    throw new CommandException($"No matching value found for parameter '{target.Name}' (Index {i}).");
                }

                Type parameterType = target.ParameterType;
                result[i] = Convert.ChangeType(matchingArgument.Value.Tokens[0], parameterType);
            }

            return result;
        }
    }
}
