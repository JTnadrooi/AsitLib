using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;

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

        public OptionInfo(ParameterInfo parameter)
            : this(ParseHelpers.GetSignature(parameter), parameter.ParameterType, parameter.GetCustomAttributes(true).Cast<Attribute>().ToArray(), parameter.HasDefaultValue, parameter.DefaultValue, new NullabilityInfoContext().Create(parameter)) { }
        public OptionInfo(string name, Type type, Attribute[] attributes)
            : this(name, type, attributes, false, null) { }
        public OptionInfo(string name, Type type, Attribute[] attributes, object? defaultValue)
            : this(name, type, attributes, true, defaultValue) { }

        private OptionInfo(string name, Type type, Attribute[] attributes, bool hasDefaultValue, object? defaultValue = null, NullabilityInfo? nullabilityInfo = null)
        {
            Name = name;
            Type = type;

            Attributes = attributes;

            _defaultValue = defaultValue;
            HasDefaultValue = hasDefaultValue;

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
        }

        public TAttribute? GetAttribute<TAttribute>() where TAttribute : Attribute
        {
            return (TAttribute?)Attributes.SingleOrDefault(a => a is TAttribute e);
        }

        public object? GetValue(string token, Type target) => ParseHelpers.GetValue(token, Type, Attributes);
        public object? GetValue(IReadOnlyList<string> tokens) => ParseHelpers.GetValue(tokens, Type, Attributes);
    }

    public static class ParameterInfoExtensions
    {
        public static OptionInfo ToOptionInfo(this ParameterInfo parameter) => new OptionInfo(parameter);
    }
}
