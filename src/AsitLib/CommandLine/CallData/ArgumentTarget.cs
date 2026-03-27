namespace AsitLib.CommandLine
{
    public readonly struct ArgumentTarget : IEquatable<ArgumentTarget>
    {
        public readonly string? Token { get; }

        public readonly int? Index { get; }

        public readonly string? Id => Token?.TrimStart('-');

        public readonly bool IsAntiTarget
        {
            get
            {
                if (Token is null) return false;

                string sanitizedId = Id!; // copy to prevent error.
                return ParseHelpers.s_antiPrefixes.Any(p => sanitizedId.StartsWith(p + "-"));
            }
        }

        public ArgumentTarget(string token)
        {
            if (token == "--" || token == "-")
                throw new ArgumentException(nameof(token));

            bool isShort = token.Length == 2 && token.StartsWith("-");

            bool isLong = token.StartsWith("--") &&
                          !token.StartsWith("---") &&
                          token.Length > 3;

            if (!isShort && !isLong)
                throw new ArgumentException($"Invalid token '{token}'.", nameof(token));

            Token = token;
            Index = null;
        }

        public ArgumentTarget(int index)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);

            Token = null;
            Index = index;
        }

        public bool IsMatchFor(OptionInfo option, int optionIndex)
        {
            bool matchesPositional = Index == optionIndex;
            bool matchesNamed = option.Ids.Contains(Id);
            bool matchesAnti = option.AntiIds.Contains(Id);

            return matchesPositional || matchesNamed || matchesAnti;
        }

        public bool IsMatchFor(GlobalOption globalOption)
            => IsMatchFor(globalOption.Option, -1);

        public override string ToString() => Token is null ? Index!.ToString()! : Token;

        public bool Equals(ArgumentTarget other)
        {
            return Token == other.Token && Index == other.Index;
        }

        public override bool Equals(object? obj)
        {
            return obj is ArgumentTarget other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 17;
                if (Token is not null) hashCode = hashCode * 23 + Token.GetHashCode();
                if (Index.HasValue) hashCode = hashCode * 23 + Index.GetHashCode();
                return hashCode;
            }
        }
    }
}
