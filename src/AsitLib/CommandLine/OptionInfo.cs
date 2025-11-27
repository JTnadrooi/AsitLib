using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

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

        public NullabilityState WriteState { get; }

        public bool HasDefaultValue { get; }

        public object[] Attributes { get; }
        public Type Type { get; }
        public string Name { get; }

        public OptionInfo(ParameterInfo parameter) : base()
        {
            _defaultValue = parameter.DefaultValue;

            HasDefaultValue = parameter.HasDefaultValue;
            Attributes = parameter.GetCustomAttributes(true);
            Type = parameter.ParameterType;
            Name = ParseHelpers.GetSignature(parameter);

            NullabilityInfoContext nullabilityInfoContext = new NullabilityInfoContext();
            NullabilityInfo nullabilityInfo = nullabilityInfoContext.Create(parameter);
            WriteState = nullabilityInfo.WriteState;
        }

        public TAttribute? GetAttribute<TAttribute>() where TAttribute : Attribute
        {
            return (TAttribute?)Attributes.SingleOrDefault(a => a is TAttribute e);
        }
    }

    public static class ParameterInfoExtensions
    {
        public static OptionInfo ToOptionInfo(this ParameterInfo parameter) => new OptionInfo(parameter);
    }
}
