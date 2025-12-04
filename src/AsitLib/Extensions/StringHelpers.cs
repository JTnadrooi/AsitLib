using AsitLib.Numerics;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
#nullable enable

namespace AsitLib
{
    public enum BetweenMethod
    {
        FirstFirst,
        FirstLast,
        LastLast,
    }

    public static class StringHelpers
    {
        public const string NULL_STRING = "null";
    }
}
