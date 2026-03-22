using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace AsitLib.CommandLine
{
    public sealed class OptionInfo
    {
        public const string IdForUnnamedOptions = "__noname__";

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

        public string Id => Ids[0];

        public string[] Ids { get; }

        public string[] AntiIds { get; }

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

        public OptionPassingPolicies PassingPolicies { get; init; }

        public OptionInfo(ParameterInfo parameter)
        {
            Attribute[] attributes = parameter.GetCustomAttributes(true).Cast<Attribute>().ToArray();

            OptionAttribute optionAttribute = (OptionAttribute?)attributes.FirstOrDefault(a => a is OptionAttribute) ?? OptionAttribute.Default;
            NullabilityInfo nullabilityInfo = new NullabilityInfoContext().Create(parameter);

            _defaultValue = parameter.HasDefaultValue ? parameter.DefaultValue : DBNull.Value;
            _implicitValue = optionAttribute.ImplicitValue;

            OptionType = parameter.ParameterType;

            Ids = (optionAttribute.Id ?? ParseHelpers.GetSignature(parameter)).ToSingleArray().Concat(optionAttribute.Aliases ?? Array.Empty<string>()).ToArray();

            PassingPolicies = optionAttribute.PassingPolicies;
            ValidationAttributes = attributes.Where(a => a is ValidationAttribute).Cast<ValidationAttribute>().ToArray();

            if (!HasDefaultValue)
                if (parameter.GetCustomAttribute<AllowNullAttribute>() is not null || nullabilityInfo?.WriteState == NullabilityState.Nullable)
                    _defaultValue = null;
                else if (OptionType.IsArray)
                    _defaultValue = Array.CreateInstance(OptionType.GetElementType()!, 0);
                else if (OptionType == typeof(bool))
                    _defaultValue = false;

            foreach (string id in Ids)
            {
                ThrowHelpers.ThrowIfInvalidOptionId(id);
            }

            if (OptionType == typeof(bool))
            {
                AntiIds = ParseHelpers.GetAntiIds(Ids);
            }
            else AntiIds = Array.Empty<string>();
        }

        private OptionInfo(Type type, string[] ids) // FromType()
        {
            Ids = ids;
            OptionType = type;
            ValidationAttributes = Array.Empty<ValidationAttribute>();
            PassingPolicies = OptionPassingPolicies.All;

            foreach (string id in ids)
            {
                ThrowHelpers.ThrowIfInvalidOptionId(id);
            }

            if (OptionType == typeof(bool))
            {
                AntiIds = ParseHelpers.GetAntiIds(Ids);
            }
            else AntiIds = Array.Empty<string>();
        }

        internal void ThrowExceptionIfInvalidValue(object? value)
        {
            foreach (ValidationAttribute attribute in ValidationAttributes)
            {
                ValidationResult? result = attribute.GetValidationResult(value, new ValidationContext(value!) { DisplayName = Id is null ? "INPUT" : $"{Id}_INPUT" });
                if (result != ValidationResult.Success) throw new CommandException($"Argument value '{value}' is invalid: {result!.ErrorMessage}");
            }
        }

        /// <summary>
        /// Converts a token to the option's type.
        /// </summary>
        /// <param name="token">The token to convert.</param>
        /// <returns>The converted value, or <see cref="ImplicitValue"/> if <paramref name="token"/> is empty.</returns>
        public object? Conform(string token) => Conform([token]);

        /// <summary>
        /// Converts a token list to the option's type.
        /// </summary>
        /// <param name="tokens">The tokens to convert.</param>
        /// <returns>The converted value, or <see cref="ImplicitValue"/> if <paramref name="tokens"/> is empty.</returns>
        public object? Conform(ReadOnlySpan<string> tokens)
        {
            object? result;

            if (tokens.Length == 0)
            {
                if (ImplicitValue is not null) result = ImplicitValue;
                else if (OptionType == typeof(bool)) result = true;
                else throw new InvalidOperationException($"Cannot convert empty token to '{this.OptionType}' type.");
            }
            else if (OptionType.IsArray)
            {
                Type elementType = OptionType.GetElementType()!;
                Array resultArray = Array.CreateInstance(elementType, tokens.Length);

                for (int i = 0; i < tokens.Length; i++) resultArray.SetValue(FromType(elementType).Conform([ParseHelpers.UnQuote(tokens[i])]), i);

                result = resultArray;
            }
            else if (tokens.Length > 1) throw new InvalidOperationException($"Cannot convert multiple tokens to '{this.OptionType}'.");
            else if (OptionType.IsEnum)
            {
                if (int.TryParse(ParseHelpers.UnQuote(tokens[0]), out int intResult)) result = Enum.ToObject(OptionType, intResult);
                else
                {
                    bool foundEnumEntry = false;
                    result = null; // this either changes or an error is thrown.

                    Dictionary<string, string> names = OptionType
                        .GetFields(BindingFlags.Public | BindingFlags.Static)
                        .Select(f => new KeyValuePair<string, string>(ParseHelpers.GetSignature(f), f.Name))
                        .ToDictionary();

                    foreach (KeyValuePair<string, string> kvp in names)
                        if (string.Equals(kvp.Key, ParseHelpers.UnQuote(tokens[0]), StringComparison.OrdinalIgnoreCase))
                        {
                            result = Enum.Parse(OptionType, kvp.Value);
                            foundEnumEntry = true;
                        }

                    if (!foundEnumEntry)
                        throw new ArgumentException($"Invalid enum value '{tokens[0]}' could not be parsed to any of [{names.ToJoinedString(", ")}].", nameof(tokens));
                }

            }
            else result = System.Convert.ChangeType(ParseHelpers.UnQuote(tokens[0]), OptionType);

            ThrowExceptionIfInvalidValue(result);

            return result;
        }

        public object? GetAntiTargetValue()
        {
            if (OptionType == typeof(bool))
            {
                return false;
            }
            else
            {
                throw new InvalidOperationException("Option does not support anti targets.");
            }
        }

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
        /// <param name="id">The value of the <see cref="OptionInfo.Id"/> property.</param>
        /// <returns>A new <see cref="OptionInfo"/> instance with the specified <paramref name="type"/>.</returns>
        public static OptionInfo FromType(
            Type type,
            string? id = null,
            OptionPassingPolicies passingPolicies = OptionPassingPolicies.All,
            object? implicitValue = null,
            ValidationAttribute[]? validationAttributes = null)
            => FromType(type, id?.ToSingleArray() ?? [IdForUnnamedOptions], passingPolicies, implicitValue, validationAttributes);

        public static OptionInfo FromType(
            Type type,
            string[] ids,
            OptionPassingPolicies passingPolicies = OptionPassingPolicies.All,
            object? implicitValue = null,
            ValidationAttribute[]? validationAttributes = null)
        {
            if (implicitValue == DBNull.Value) throw new Exception($"{nameof(DBNull.Value)} is not valid as implicit value.");
            if (ids.Length == 0) throw new ArgumentException("Array cannot be empty.", nameof(ids));

            return new OptionInfo(type, ids)
            {
                PassingPolicies = passingPolicies,
                _implicitValue = implicitValue,
                ValidationAttributes = validationAttributes ?? Array.Empty<ValidationAttribute>()
            };
        }

        public override string ToString()
        {
            return $"{{Ids: [{Ids.ToJoinedString(", ")}], Type: '{OptionType}'}}";
        }
    }
}
