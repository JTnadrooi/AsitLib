using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib.CommandLine
{
    public class ParameterNameAttribute : Attribute
    {
        public string Name { get; set; }
        public ParameterNameAttribute(string name) => Name = name;
    }
}
