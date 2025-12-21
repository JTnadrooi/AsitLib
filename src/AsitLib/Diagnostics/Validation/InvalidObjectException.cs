namespace AsitLib.Diagnostics
{
    public class InvalidObjectException : Exception
    {
        public IReadOnlyList<InvalidReason> Reasons { get; }

        public InvalidObjectException(string message) : this(new InvalidReason(message)) { }

        public InvalidObjectException(params IReadOnlyList<InvalidReason> reasons) : base($"Invalid state, reasons: '{reasons.ToJoinedString(", ")}'.")
        {
            if (reasons.Count == 0) throw new ArgumentException("'reasons' cannot be empty.", nameof(reasons));

            Reasons = reasons;
        }
    }

    public struct InvalidReason
    {
        public string Message { get; }

        public InvalidReason(string message)
        {
            ArgumentException.ThrowIfNullOrEmpty(message);

            Message = message;
        }

        public static bool operator ==(InvalidReason left, InvalidReason right) => left.Message == right.Message;
        public static bool operator !=(InvalidReason left, InvalidReason right) => left.Message != right.Message;

        public static implicit operator InvalidReason(string msg) => new InvalidReason(msg);
        public static implicit operator string(InvalidReason invalidReason) => invalidReason.Message;

        public override bool Equals(object? obj) => obj is InvalidReason other && this == other;
        public override int GetHashCode() => Message?.GetHashCode() ?? 0;
        public override string ToString() => $"{{Message: '{Message}'}}";
    }
}
