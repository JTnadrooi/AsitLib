using System.Collections.Immutable;

namespace AsitLib.CommandLine
{
    public sealed class CallInfo
    {
        public ImmutableArray<Argument> Arguments { get; }

        public object?[] Conformed { get; }
        public CommandInfo Command { get; }
        public GlobalOption[] GlobalOptions { get; }

        internal CallInfo(CommandEngine engine, ReadOnlySpan<Argument> arguments, object?[] conformed, CommandInfo command, GlobalOption[] globalOptions)
        {
            bool callsGenericFlag = command.Id.StartsWith("-");

            if (callsGenericFlag && arguments.Length != 0) throw new CommandException("Cannot call generic flag with arguments.");
            if (callsGenericFlag && !ParseHelpers.IsValidGenericFlagCall(command.Id)) throw new CommandException("Invalid generic flag call.");

            if (arguments.HasDuplicates(a => a.Target)) throw new CommandArgumentException("Cannot have duplicate argument targets.");

            Arguments = arguments.ToImmutableArray();
            Conformed = conformed;
            Command = command;
            GlobalOptions = globalOptions;
        }

        public override string ToString() => $"{{Id: '{Command.Id}', Expected parameters: [{Arguments.Select(a => a.Target).ToJoinedString(", ")}]}}";
    }
}
