using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib.Tests
{
    public class DummyGlobalOption : GlobalOption
    {
        public DummyGlobalOption(string longFormId, string description = "", string? shorthandId = null, OptionInfo? option = null) : base(longFormId, description, shorthandId, option)
        {
        }
    }
}
