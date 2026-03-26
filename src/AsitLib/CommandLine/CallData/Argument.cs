using System.Collections.Immutable;

namespace AsitLib.CommandLine
{
    public sealed class Argument
    {
        public ArgumentTarget Target { get; }
        public ImmutableArray<string> Tokens { get; }

        public bool CanTargetChildCommand => Target.Id is null && Tokens.Length == 1 && !ParseHelpers.IsQuoted(Tokens[0]);

        public Argument(ArgumentTarget target, ReadOnlySpan<string> tokens)
        {
            Target = target;
            Tokens = tokens.ToImmutableArray();
        }

        public override string ToString() =>
            $"{{Target: '{Target}', Tokens: [{string.Join(", ", Tokens)}]}}";

        public override bool Equals(object? obj)
        {
            if (obj is not Argument other) return false;

            return Target.Equals(other.Target) && Tokens.SequenceEqual(other.Tokens);
        }

        public override int GetHashCode()
        {
            HashCode hashCode = new HashCode();
            hashCode.Add(Target);
            foreach (string token in Tokens) hashCode.Add(token);
            return hashCode.ToHashCode();
        }

        public static bool operator ==(Argument? left, Argument? right)
        {
            if (ReferenceEquals(left, right))
                return true;

            if (left is null || right is null)
                return false;

            return left.Equals(right);
        }

        public static bool operator !=(Argument? left, Argument? right)
        {
            return !(left == right);
        }
    }
}
