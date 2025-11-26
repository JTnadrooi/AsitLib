using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib.Diagnostics
{
    public interface IValidatable
    {
        public InvalidReason[] GetInvalidReasons();
    }

    public static class ValidatableExtensions
    {
        public static bool IsValid(this IValidatable validatableObj) => validatableObj.IsValid(out _);
        public static bool IsValid(this IValidatable validatableObj, out InvalidReason[] invalidReasons)
            => (invalidReasons = validatableObj.GetInvalidReasons()).Length != 0;
        public static void ThrowIfInvalid(this IValidatable validatableObj)
        {
            if (validatableObj.IsValid(out InvalidReason[] invalidReasons))
            {
                throw new InvalidOperationException($"Invalid state, reasons: '{invalidReasons.ToJoinedString(", ")}'.");
            }
        }
    }

    public class InvalidStateException : Exception
    {
        public InvalidStateException(string message) : base(message) { }
    }

    public struct InvalidReason
    {
        public string Message { get; }

        public InvalidReason(string msg) => Message = msg;

        public static bool operator ==(InvalidReason left, InvalidReason right) => left.Message == right.Message;
        public static bool operator !=(InvalidReason left, InvalidReason right) => left.Message != right.Message;

        public static implicit operator InvalidReason(string msg) => new InvalidReason(msg);
        public static implicit operator string(InvalidReason invalidReason) => invalidReason.Message;

        public override bool Equals(object? obj) => obj is InvalidReason other && this == other;
        public override int GetHashCode() => Message?.GetHashCode() ?? 0;
        public override string ToString() => $"InvalidReason: {Message}";
    }

}
