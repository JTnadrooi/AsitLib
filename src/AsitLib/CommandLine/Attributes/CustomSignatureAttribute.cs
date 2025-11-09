using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib.CommandLine
{
    public class CustomSignatureAttribute : Attribute
    {
        public string? Name { get; }
        /// <summary>
        ///  Initializes an instance of <see cref="CustomSignatureAttribute"/>.
        /// </summary>
        /// <param name="name">Overwrit</param>
        public CustomSignatureAttribute(string? name) => Name = name;
    }
}
