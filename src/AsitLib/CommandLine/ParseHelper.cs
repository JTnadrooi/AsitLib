using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AsitLib.CommandLine
{
    internal readonly struct ArgumentTarget
    {
        public readonly string? ParameterName;
        public readonly int? ParameterIndex;
        public readonly bool ShortHand => UsesExplicitName && !ParameterName!.StartsWith("--");

        public bool UsesExplicitName => ParameterName != null;

        public ArgumentTarget(string parameterName)
        {
            ParameterName = parameterName;
            ParameterIndex = null;
        }

        public ArgumentTarget(int parameterIndex)
        {
            ParameterName = null;
            ParameterIndex = parameterIndex;
        }

        public override string ToString() => UsesExplicitName ? ParameterName! : ParameterIndex!.ToString()!;

    }

    internal readonly struct Argument
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

    internal readonly struct ArgumentsInfo
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

    internal static class ParseHelper
    {
        public static string ToKebabCase(this string str) => Regex.Replace(str, "(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z0-9])", "-$1", RegexOptions.Compiled).Trim().ToLower();

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
    }
}
