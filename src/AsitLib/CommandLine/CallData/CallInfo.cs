using System.Collections.Immutable;

namespace AsitLib.CommandLine
{
    public sealed class CallInfo
    {
        public string CommandId { get; }
        public string SanitizedCommandId { get; }
        public ImmutableArray<Argument> Arguments { get; }
        public bool CallsGenericFlag { get; }

        public CallInfo(string commandId, IReadOnlyList<Argument> arguments)
        {
            bool callsGenericFlag = commandId.StartsWith("-");

            if (callsGenericFlag && arguments.Count != 0) throw new CommandException("Cannot call generic flag with arguments.");
            if (callsGenericFlag && !ParseHelpers.IsValidGenericFlagCall(commandId)) throw new CommandException("Invalid generic flag call.");

            CommandId = commandId;
            SanitizedCommandId = CommandId.TrimStart('-');
            Arguments = arguments.ToImmutableArray();
            CallsGenericFlag = callsGenericFlag;
        }

        public override string ToString() => $"{{Id: '{CommandId}', Expected parameters: [{Arguments.Select(a => a.Target).ToJoinedString(", ")}]}}";
    }
}
