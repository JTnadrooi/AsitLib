using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using AsitLib;
using System.Diagnostics.CodeAnalysis;
#nullable enable

namespace AsitLib
{
    public sealed class FormatterManipulator : IUniManipulator
    {
        ICustomFormatter CustomFormatter { get; }
        public string? Format { get; }
        public IFormatProvider? FormatProvider { get; }
        public FormatterManipulator(ICustomFormatter customFormatter, string? format, IFormatProvider? formatProvider)
        {
            CustomFormatter = customFormatter;
            Format = format;
            FormatProvider = formatProvider;
        }
        public void Dispose() { }
        [return: NotNull]
        public string Maniputate(string? unmanipulatedInput, object? arg)
            => CustomFormatter.Format(Format, unmanipulatedInput, FormatProvider);
    }
    public sealed class DelegateManipulator : IUniManipulator
    {
        public Func<string?, string> Function { get; }
        public DelegateManipulator(Func<string?, string> function)
        {
            Function = function;
        }
        [return: NotNull]
        public string Maniputate(string? unmanipulatedInput, object? arg)
            => Function.Invoke(unmanipulatedInput);
        public void Dispose() { }
    }
}
