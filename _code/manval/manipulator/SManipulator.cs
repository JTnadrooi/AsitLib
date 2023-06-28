using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using AsitLib;
#nullable enable

namespace AsitLib
{
    public static class SManipulator
    {
        public static IUniManipulator Empty => new DelegateManipulator((s) => s ?? "Null");
        public static DelegateManipulator FromFunction(Func<string?, string> function) => new DelegateManipulator(function);
        public static FormatterManipulator FromCustomFormatter(ICustomFormatter customFormatter, string? format, IFormatProvider? formatProvider) => new FormatterManipulator(customFormatter, format, formatProvider);
    }
}
