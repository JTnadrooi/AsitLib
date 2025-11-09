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
        public string? ShortHand { get; }
        public ShorthandAttribute(string? shortHand = null) => ShortHand = shortHand;
    }
}
