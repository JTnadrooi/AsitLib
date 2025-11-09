using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib.CommandLine.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class ShorthandAttribute : Attribute
    {
        public string? Shorthand { get; }
        public ShorthandAttribute(string? shorthand = null) => Shorthand = shorthand;
    }
}
