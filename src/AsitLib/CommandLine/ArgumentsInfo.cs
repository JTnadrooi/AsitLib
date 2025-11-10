using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib.CommandLine
{
    public readonly struct ArgumentTarget
    {
        public readonly string? ParameterToken { get; }
        public readonly int? ParameterIndex { get; }
        public readonly bool IsShorthand => UsesExplicitName && !ParameterToken!.StartsWith("--");
        public readonly bool IsLongForm => UsesExplicitName && ParameterToken!.StartsWith("--");
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

        public override bool Equals(object? obj)
        {
            if (obj is ArgumentTarget other) return (ParameterToken == other.ParameterToken && ParameterIndex == other.ParameterIndex);
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 17;
                if (ParameterToken != null) hashCode = hashCode * 23 + ParameterToken.GetHashCode();
                if (ParameterIndex.HasValue) hashCode = hashCode * 23 + ParameterIndex.GetHashCode();
                return hashCode;
            }
        }
    }

    public readonly struct Argument
    {
        public readonly ArgumentTarget Target { get; }
        public readonly ReadOnlyCollection<string> Tokens { get; }

        public Argument(ArgumentTarget target, string[] tokens)
        {
            Target = target;
            Tokens = tokens.AsReadOnly();
        }

        public bool TryGetSingle(out string? token) => (token = Tokens.SingleOrDefault()) == null;

        public override string ToString() => $"{{Target: '{Target}', Tokens: [{Tokens.ToJoinedString(", ")}]}}";
    }

    public readonly struct ArgumentsInfo
    {
        public readonly string CommandId { get; }
        public readonly ReadOnlyCollection<Argument> Arguments { get; }

        public ArgumentsInfo(string commandId, Argument[] arguments)
        {
            CommandId = commandId;
            Arguments = arguments.AsReadOnly();
        }

        //internal string ToDisplayString()
        //{
        //    StringBuilder sb = new StringBuilder();
        //    sb.AppendLine("ArgumentsInfo:");
        //    sb.AppendLine($"\tCommandId: {this.CommandId}");
        //    sb.AppendLine("\tArguments:");

        //    for (int i = 0; i < Arguments.Length; i++)
        //    {
        //        Argument arg = Arguments[i];
        //        sb.AppendLine($"\t\tTarget: {arg.Target}");
        //        sb.AppendLine($"\t\tShorthand: {arg.Target.IsShorthand}");
        //        sb.AppendLine("\t\tTokens:");
        //        foreach (string token in arg.Tokens) sb.AppendLine($"\t\t\t{token}");
        //    }

        //    return sb.ToString();
        //}

        public override string ToString() => $"{{Id: '{CommandId}', Expected parameters: [{Arguments.Select(a => a.Target).ToJoinedString(", ")}]}}";

        public static ArgumentsInfo Parse(string args) => ParseHelpers.Parse(args);
        public static ArgumentsInfo Parse(string[] args) => ParseHelpers.Parse(args);
    }
}
