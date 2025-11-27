using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib.CommandLine
{
    public readonly struct ArgumentTarget
    {
        public readonly string? OptionToken { get; }
        public readonly int? OptionIndex { get; }
        public readonly bool IsShorthand => UsesExplicitName && !OptionToken!.StartsWith("--");
        public readonly bool IsLongForm => UsesExplicitName && OptionToken!.StartsWith("--");
        public readonly string? SanitizedOptionToken => OptionToken?.TrimStart('-');

        public bool UsesExplicitName => OptionToken is not null;

        public ArgumentTarget(string optionName)
        {
            OptionToken = optionName;
            OptionIndex = null;
        }

        public ArgumentTarget(int optionIndex)
        {
            if (optionIndex < 0) throw new ArgumentException(nameof(optionIndex));

            OptionToken = null;
            OptionIndex = optionIndex;
        }

        public bool TargetsFlag(GlobalOptionHandler flagHandler)
            => (IsShorthand && flagHandler.HasShorthandId && flagHandler.ShorthandId == SanitizedOptionToken) ||
                (IsLongForm && flagHandler.LongFormId == SanitizedOptionToken);

        public override string ToString() => UsesExplicitName ? OptionToken! : OptionIndex!.ToString()!;

        public override bool Equals(object? obj)
        {
            if (obj is ArgumentTarget other) return (OptionToken == other.OptionToken && OptionIndex == other.OptionIndex);
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 17;
                if (OptionToken is not null) hashCode = hashCode * 23 + OptionToken.GetHashCode();
                if (OptionIndex.HasValue) hashCode = hashCode * 23 + OptionIndex.GetHashCode();
                return hashCode;
            }
        }
    }

    public sealed class Argument
    {
        public ArgumentTarget Target { get; }
        public IReadOnlyList<string> Tokens { get; }

        public Argument(ArgumentTarget target, IReadOnlyList<string> tokens)
        {
            Target = target;
            Tokens = tokens;
        }

        public bool TryGetSingle(out string? token) => (token = Tokens.SingleOrDefault()) is null;

        public override string ToString() => $"{{Target: '{Target}', Tokens: [{Tokens.ToJoinedString(", ")}]}}";
    }

    public sealed class ArgumentsInfo
    {
        public string CommandId { get; }
        public string SanitizedCommandId { get; }
        public IReadOnlyList<Argument> Arguments { get; }
        public bool CallsGenericFlag { get; }

        public ArgumentsInfo(string commandId, IReadOnlyList<Argument> arguments, bool callsGenericFlag)
        {
            if (callsGenericFlag && arguments.Count != 0) throw new CommandException("Arguments call command as generic flag, but argument count is not 0.");
            if (callsGenericFlag != commandId.StartsWith("-")) throw new CommandException("Invalid generic call command id relation.");

            CommandId = commandId;
            SanitizedCommandId = CommandId.TrimStart('-');
            Arguments = arguments;
            CallsGenericFlag = callsGenericFlag;
        }

        public override string ToString() => $"{{Id: '{CommandId}', Expected parameters: [{Arguments.Select(a => a.Target).ToJoinedString(", ")}]}}";

        public static ArgumentsInfo Parse(string args) => ParseHelpers.Parse(args);
        public static ArgumentsInfo Parse(string[] args) => ParseHelpers.Parse(args);
    }
}
