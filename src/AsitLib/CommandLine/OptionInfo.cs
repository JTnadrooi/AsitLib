using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace AsitLib.CommandLine
{
    public sealed class OptionInfo
    {
        public const string NameForUnnamedOptions = "__noname__";

        private object? _defaultValue;

        public object? DefaultValue
        {
            get
            {
                if (!HasDefaultValue) throw new InvalidOperationException("Cannot get default value from a non-optional option.");
                return _defaultValue;
            }

            init
            {
                if (value == DBNull.Value) throw new Exception($"{nameof(DBNull.Value)} is not valid as default value.");
                _defaultValue = value;
            }
        }

        public ValidationAttribute[] ValidationAttributes { get; init; }

        public bool HasDefaultValue => _defaultValue != DBNull.Value;

        public Type OptionType { get; }

        public string Name { get; }

        private object? _implicitValue;

        [DisallowNull]
        public object? ImplicitValue
        {
            get => _implicitValue;
            init
            {
                if (value == DBNull.Value) throw new Exception($"{nameof(DBNull.Value)} is not valid as implicit value.");
                _implicitValue = value ?? throw new InvalidOperationException("Cannot set null as implicit value.");
            }
        }

        public string? Shorthand { get; init; }

        public string? AntiParameterName { get; init; }

        public OptionPassingPolicies PassingPolicies { get; init; }

        public OptionInfo(ParameterInfo parameter)
        {
            Attribute[] attributes = parameter.GetCustomAttributes(true).Cast<Attribute>().ToArray();

            OptionAttribute optionAttribute = (OptionAttribute?)attributes.FirstOrDefault(a => a is OptionAttribute) ?? OptionAttribute.Default;
            NullabilityInfo nullabilityInfo = new NullabilityInfoContext().Create(parameter);

            _defaultValue = parameter.HasDefaultValue ? parameter.DefaultValue : DBNull.Value;
            _implicitValue = optionAttribute.ImplicitValue;

            OptionType = parameter.ParameterType;
            Name = optionAttribute.Name ?? ParseHelpers.GetSignature(parameter);

            Shorthand = optionAttribute.Shorthand;
            AntiParameterName = optionAttribute.AntiParameterName;

            PassingPolicies = optionAttribute.PassingPolicies;
            ValidationAttributes = attributes.Where(a => a is ValidationAttribute).Cast<ValidationAttribute>().ToArray();

            if (!HasDefaultValue)
                if (parameter.GetCustomAttribute<AllowNullAttribute>() is not null || nullabilityInfo?.WriteState == NullabilityState.Nullable)
                    _defaultValue = null;
                else if (OptionType.IsArray)
                    _defaultValue = Array.CreateInstance(OptionType.GetElementType()!, 0);
                else if (OptionType == typeof(bool))
                    _defaultValue = false;
        }

        private OptionInfo(Type type, string name = NameForUnnamedOptions) // FromType()
        {
            Name = name;
            OptionType = type;
            ValidationAttributes = Array.Empty<ValidationAttribute>();
            PassingPolicies = OptionPassingPolicies.All;
        }

        internal void ThrowExceptionIfNoName()
        {
            if (Name is null) throw new InvalidOperationException("This operation is not valid on an unnamed OptionInfo.");
        }

        internal void ThrowExceptionIfInvalidValue(object? value)
        {
            foreach (ValidationAttribute attribute in ValidationAttributes)
            {
                ValidationResult? result = attribute.GetValidationResult(value, new ValidationContext(value!) { DisplayName = Name is null ? "INPUT" : $"{Name}_INPUT" });
                if (result != ValidationResult.Success) throw new CommandException($"Argument value '{value}' is invalid: {result!.ErrorMessage}");
            }
        }

        public object? GetValue(string token) => ParseHelpers.GetValue(token, this);
        public object? GetValue(IReadOnlyList<string> tokens) => ParseHelpers.GetValue(tokens, this);

        public OptionPassingPolicies GetInheritedPassingPoliciesFromContext(CommandContext? context = null)
            => GetInheritedPassingPolicies(context?.Engine, context?.Command);
        public OptionPassingPolicies GetInheritedPassingPolicies(CommandEngine? engine = null, CommandInfo? command = null)
        {
            OptionPassingPolicies? ToNullIfNone(OptionPassingPolicies? passingPolicies) => (passingPolicies == OptionPassingPolicies.None || passingPolicies is null) ? null : passingPolicies;

            return ToNullIfNone(engine?.PassingPolicies) ?? ToNullIfNone(command?.PassingPolicies) ?? PassingPolicies;
        }

        /// <summary>
        /// Creates a new <see cref="OptionInfo"/> instance with the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name">The value of the <see cref="OptionInfo.Name"/> property.</param>
        /// <returns>A new <see cref="OptionInfo"/> instance with the specified <paramref name="type"/>.</returns>
        public static OptionInfo FromType(
            Type type,
            string name = NameForUnnamedOptions,
            OptionPassingPolicies passingPolicies = OptionPassingPolicies.All,
            object? implicitValue = null,
            string? shorthand = null,
            string? antiParameterName = null,
            ValidationAttribute[]? validationAttributes = null)
        {
            if (implicitValue == DBNull.Value) throw new Exception($"{nameof(DBNull.Value)} is not valid as implicit value.");

            return new OptionInfo(type, name)
            {
                PassingPolicies = passingPolicies,
                _implicitValue = implicitValue,
                Shorthand = shorthand,
                AntiParameterName = antiParameterName,
                ValidationAttributes = validationAttributes ?? Array.Empty<ValidationAttribute>()
            };
        }
    }
}
