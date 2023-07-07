using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using AsitLib;
using System.Diagnostics.CodeAnalysis;
using System.Collections;
#nullable enable

namespace AsitLib
{
    public sealed class FormatterManipulator : IUniManipulator<string, string>
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
        [return: NotNull]
        public object Maniputate(object? input, string? arg) => Maniputate((string?)input, arg);
        [return: NotNull]
        public string Maniputate(string? input, string? arg)
            => CustomFormatter.Format(Format, input, FormatProvider);
        public void Dispose() { }
    }
    public sealed class DelegateManipulator<TIn, TOut> : IUniManipulator<TIn, TOut>
    {
        public Func<TIn, TOut> Function { get; }
        public DelegateManipulator(Func<TIn, TOut> function)
        {
            Function = function;
        }
        [return: NotNull]
        public object Maniputate(object? input, string? arg) => Maniputate((string?)input, arg);
        [return: NotNull]
        public TOut Maniputate(TIn input, string? arg)
            => Function.Invoke(input)!;
        public void Dispose() { }
    }
    public sealed class ObjManipulator : IUniManipulator<object, object>
    {
        public IUniManipulator Source { get; }
        public ObjManipulator(IUniManipulator source)
        {
            Source = source;
        }
        [return: NotNull]
        public object Maniputate(object? input, string? arg)
            => Source.Maniputate(input, arg);
        public void Dispose() { }
    }
}
