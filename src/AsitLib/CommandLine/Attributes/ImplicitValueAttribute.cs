using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib.CommandLine.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class ImplicitValueAttribute : Attribute
    {
        public object Value { get; }
        public ImplicitValueAttribute(object value)
        {
            Value = value;
        }
    }
}
