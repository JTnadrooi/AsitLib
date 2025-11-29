using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib.CommandLine
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class OptionAttribute : Attribute
    {
        private object? _implicitValue;

        public bool UsesImplicitValue { get; private set; }

        public object? ImplicitValue
        {
            get => _implicitValue;
            init
            {
                _implicitValue = value;
                UsesImplicitValue = true;
            }
        }

        public string? Shorthand { get; init; }

        public string? Name { get; init; }

        public string? AntiParameterName { get; init; }

        public OptionAttribute() { }

        public static OptionAttribute Default { get; } = new OptionAttribute();
    }
}
