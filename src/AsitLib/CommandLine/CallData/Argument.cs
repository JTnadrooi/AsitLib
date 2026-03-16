using System.Collections.Immutable;

namespace AsitLib.CommandLine
{
    public sealed class Argument
    {
        public ArgumentTarget Target { get; }
        public ImmutableArray<string> Tokens { get; }

        public bool CanTargetSubcommand => Target.Id is null && Tokens.Length == 1 && !ParseHelpers.IsQuoted(Tokens[0]);

        public Argument(ArgumentTarget target, IReadOnlyList<string> tokens)
        {
            Target = target;
            Tokens = tokens.ToImmutableArray();
        }

        public override string ToString() =>
            $"{{Target: '{Target}', Tokens: [{string.Join(", ", Tokens)}]}}";
    }
}
