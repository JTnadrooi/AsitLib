namespace AsitLib.Diagnostics
{
    public interface IValidatable
    {
        public InvalidReason[] GetInvalidReasons();
    }

    public static class ValidatableExtensions
    {
        public static bool IsValid(this IValidatable validatableObj) => validatableObj.IsValid(out _);
        public static bool IsValid(this IValidatable validatableObj, out InvalidReason[] reasons)
            => (reasons = validatableObj.GetInvalidReasons()).Length != 0;

        public static void ThrowIfInvalid(this IValidatable validatableObj)
        {
            if (validatableObj.IsValid(out InvalidReason[] reasons))
            {
                throw new InvalidObjectException(reasons);
            }
        }
    }
}
