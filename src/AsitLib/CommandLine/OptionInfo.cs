using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace AsitLib.CommandLine
{
    public sealed class OptionInfo
    {
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

        [DisallowNull]
        public string? Name { get; }

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

        internal OptionInfo(ParameterInfo parameter)
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

        public OptionInfo(Type optionType)
        {
            OptionType = optionType;
            ValidationAttributes = Array.Empty<ValidationAttribute>();
            PassingPolicies = OptionPassingPolicies.All;
        }

        public OptionInfo(string name, Type optionType)
        {
            Name = name;
            OptionType = optionType;
            ValidationAttributes = Array.Empty<ValidationAttribute>();
            PassingPolicies = OptionPassingPolicies.All;
        }

        public void ThrowExceptionIfInvalidValue(object? value)
        {
            foreach (ValidationAttribute attribute in ValidationAttributes)
            {
                ValidationResult? result = attribute.GetValidationResult(value, new ValidationContext(value!) { DisplayName = Name is null ? "INPUT" : $"{Name}_INPUT" });
                if (result != ValidationResult.Success) throw new CommandException($"Argument value '{value}' is invalid: {result!.ErrorMessage}");
            }
        }

        public void ThrowExceptionIfNoName()
        {
            if (Name is null) throw new InvalidOperationException("This operation is not valid on an unnamed OptionInfo.");
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
    }

    public static class ParameterInfoExtensions
    {
        public static OptionInfo ToOptionInfo(this ParameterInfo parameter) => new OptionInfo(parameter);
    }
}
