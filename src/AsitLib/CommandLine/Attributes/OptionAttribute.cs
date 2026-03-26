using System.Diagnostics.CodeAnalysis;

namespace AsitLib.CommandLine
{

    [AttributeUsage(AttributeTargets.Parameter)]
    public class OptionAttribute : Attribute
    {
        private object? _implicitValue;

        [DisallowNull]
        public object? ImplicitValue
        {
            get => _implicitValue;
            init
            {
                if (value == DBNull.Value) throw new Exception($"{nameof(DBNull.Value)} is not valid as implicit value.");
                _implicitValue = value ?? throw new ArgumentException("Cannot set null as implicit value.", nameof(value));
            }
        }

        public string? Id { get; init; }

        public string[]? Aliases { get; init; }

        public OptionAttribute() { }

        public static OptionAttribute Default { get; } = new OptionAttribute();
    }
}
