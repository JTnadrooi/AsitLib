using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib.CommandLine
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class AllowAntiArgumentAttribute : Attribute
    {
        public AllowAntiArgumentAttribute() { }
    }
}
