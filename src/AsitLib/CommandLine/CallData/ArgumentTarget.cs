namespace AsitLib.CommandLine
{
    public readonly struct ArgumentTarget : IEquatable<ArgumentTarget>
    {
        public readonly string? Id { get; }

        public readonly int? Index { get; }

        public readonly bool IsShorthand => Id is not null && !Id.StartsWith("--");

        public readonly bool IsLongForm => Id is not null && Id!.StartsWith("--");

        public readonly string? SanitizedId => Id?.TrimStart('-');

        public readonly bool IsAntiTarget
        {
            get
            {
                if (Id is null) return false;

                string sanitizedId = SanitizedId!; // copy to prevent error.
                return ParseHelpers.s_antiPrefixes.Any(p => sanitizedId.StartsWith(p + "-"));
            }
        }

        public ArgumentTarget(string id)
        {
            Id = id;
            Index = null;
        }

        public ArgumentTarget(int index)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);

            Id = null;
            Index = index;
        }

        public bool IsMatchFor(OptionInfo option, int optionIndex, CommandContext? context = null)
        {
            OptionPassingPolicies passingPolicies = option.GetInheritedPassingPoliciesFromContext(context);

            bool matchesPositional = passingPolicies.HasFlag(OptionPassingPolicies.Positional) && (Index == optionIndex);
            bool matchesNamed = passingPolicies.HasFlag(OptionPassingPolicies.Named) && (option.Ids.Contains(SanitizedId));
            bool matchesAnti = passingPolicies.HasFlag(OptionPassingPolicies.Named) && (option.AntiIds.Contains(SanitizedId));

            return matchesPositional || matchesNamed || matchesAnti;
        }

        public bool IsMatchFor(GlobalOption globalOption)
            => (IsShorthand && globalOption.HasShorthandId && globalOption.ShorthandId == SanitizedId) ||
                (IsLongForm && globalOption.LongFormId == SanitizedId);

        public override string ToString() => Id is null ? Index!.ToString()! : Id;

        public bool Equals(ArgumentTarget other)
        {
            return Id == other.Id && Index == other.Index;
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
                if (Id is not null) hashCode = hashCode * 23 + Id.GetHashCode();
                if (Index.HasValue) hashCode = hashCode * 23 + Index.GetHashCode();
                return hashCode;
            }
        }
    }
}
