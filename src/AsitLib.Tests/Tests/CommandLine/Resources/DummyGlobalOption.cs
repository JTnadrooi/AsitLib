using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib.Tests
{
    public class DummyGlobalOption : GlobalOption
    {
        public DummyGlobalOption(OptionInfo option, string description) : base(option, description)
        {
        }
    }
}
