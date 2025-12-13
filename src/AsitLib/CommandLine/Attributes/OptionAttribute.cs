using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                _implicitValue = value ?? throw new InvalidOperationException("Cannot set null as implicit value.");
            }
        }

        public string? Shorthand { get; init; }

        public string? Name { get; init; }

        public string? AntiParameterName { get; init; }

        public OptionPassingPolicies PassingPolicies { get; init; } = OptionPassingPolicies.All;

        public OptionAttribute() { }

        public static OptionAttribute Default { get; } = new OptionAttribute();
    }
}
