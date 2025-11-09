using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
}
