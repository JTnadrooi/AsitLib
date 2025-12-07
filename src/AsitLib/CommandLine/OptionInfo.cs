using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;

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
        }

        public bool HasDefaultValue { get; }

        public Attribute[] Attributes { get; }
        public Type Type { get; }
        public string Name { get; }

        public OptionAttribute OptionAttribute { get; }

        public OptionInfo(ParameterInfo parameter)
            : this(ParseHelpers.GetSignature(parameter), parameter.ParameterType, parameter.HasDefaultValue, parameter.DefaultValue, parameter.GetCustomAttributes(true).Cast<Attribute>().ToArray(), new NullabilityInfoContext().Create(parameter)) { }
        public OptionInfo(string name, Type type, Attribute[]? attributes = null)
            : this(name, type, false, attributes: attributes) { }
        public OptionInfo(string name, Type type, object? defaultValue, Attribute[]? attributes = null)
            : this(name, type, true, defaultValue: defaultValue, attributes: attributes) { }

        private OptionInfo(string name, Type type, bool hasDefaultValue, object? defaultValue = null, Attribute[]? attributes = null, NullabilityInfo? nullabilityInfo = null)
        {
            attributes ??= Array.Empty<Attribute>();

            Name = name;
            Type = type;

            Attributes = attributes;

            _defaultValue = defaultValue;
            HasDefaultValue = hasDefaultValue;

            OptionAttribute = (OptionAttribute?)attributes.SingleOrDefault(a => a is OptionAttribute oa) ?? AsitLib.CommandLine.OptionAttribute.Default;

            if (!HasDefaultValue)
                if (GetAttribute<AllowNullAttribute>() is not null || nullabilityInfo?.WriteState == NullabilityState.Nullable)
                {
                    HasDefaultValue = true;
                    _defaultValue = null;
                }
                else if (Type.IsArray)
                {
                    HasDefaultValue = true;
                    _defaultValue = Array.CreateInstance(Type.GetElementType()!, 0);
                }
                else if (Type == typeof(bool))
                {
                    HasDefaultValue = true;
                    _defaultValue = false;
                }

            if (OptionAttribute.Name is not null) Name = OptionAttribute.Name;
        }

        public TAttribute? GetAttribute<TAttribute>() where TAttribute : Attribute
        {
            return (TAttribute?)Attributes.SingleOrDefault(a => a is TAttribute e);
        }

        public object? GetValue(string token) => ParseHelpers.GetValue(token, Type, Attributes, OptionAttribute.ImplicitValue);
        public object? GetValue(IReadOnlyList<string> tokens) => ParseHelpers.GetValue(tokens, Type, Attributes, OptionAttribute.ImplicitValue);
    }

    public static class ParameterInfoExtensions
    {
        public static OptionInfo ToOptionInfo(this ParameterInfo parameter) => new OptionInfo(parameter);
    }
}
