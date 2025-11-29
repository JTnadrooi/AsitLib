using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib.CommandLine
{
    [AttributeUsage(AttributeTargets.All & ~(AttributeTargets.Parameter | AttributeTargets.Constructor | AttributeTargets.ReturnValue))]
    public class SignatureAttribute : Attribute
    {
        public string Name { get; }
        /// <summary>
        ///  Initializes an instance of <see cref="SignatureAttribute"/>.
        /// </summary>
        /// <param name="name">Overwrit</param>
        public SignatureAttribute(string name) => Name = name;
    }
}
